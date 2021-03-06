using System;
using System.Linq;
using System.Linq.Expressions;
using FubuCore;
using FubuCore.Reflection;
using FubuMVC.Core.Diagnostics;
using FubuMVC.Core.Registration;
using FubuMVC.Core.Registration.Conventions;
using FubuMVC.Core.Registration.DSL;
using FubuMVC.Core.View.WebForms;

namespace FubuMVC.Core
{
    [Obsolete("Should probably dump this in favor of the IFubuRegistryExtension")]
    public interface IRegistryModification
    {
        void Modify(FubuRegistry registry);
    }

    public partial class FubuRegistry
    {
        private readonly TypeResolver _typeResolver = new TypeResolver();

        public RouteConventionExpression Routes
        {
            get { return new RouteConventionExpression(_routeResolver, this); }
        }

        public OutputDeterminationExpression Output
        {
            get { return new OutputDeterminationExpression(this); }
        }

        public ViewExpression Views
        {
            get { return new ViewExpression(_viewAttacher); }
        }

        public PoliciesExpression Policies
        {
            get { return new PoliciesExpression(_policies, _systemPolicies); }
        }

        public ModelsExpression Models
        {
            get { return new ModelsExpression(addExplicit); }
        }

        public AppliesToExpression Applies
        {
            get { return new AppliesToExpression(_types); }
        }

        public ActionCallCandidateExpression Actions
        {
            get { return new ActionCallCandidateExpression(_behaviorMatcher, _types, _actionSourceMatcher); }
        }

        public TypeResolver TypeResolver
        {
            get { return _typeResolver; }
        }

        public void UsingObserver(IConfigurationObserver observer)
        {
            _observer = observer;
        }

        public void Services(Action<IServiceRegistry> configure)
        {
            var action = new LambdaConfigurationAction(g => configure(g.Services));
            _explicits.Add(action);
        }

        public void ApplyConvention<TConvention>()
            where TConvention : IConfigurationAction, new()
        {
            ApplyConvention(new TConvention());
        }

        public void ApplyConvention<TConvention>(TConvention convention)
            where TConvention : IConfigurationAction
        {
            _conventions.Add(convention);
        }

        public void HomeIs<TController>(Expression<Action<TController>> controllerAction)
        {
            var method = ReflectionHelper.GetMethod(controllerAction);
            _routeResolver.RegisterUrlPolicy(new DefaultRouteMethodBasedUrlPolicy(method));
        }

        public void HomeIs<TModel>()
        {
            _routeResolver.RegisterUrlPolicy(new DefaultRouteInputTypeBasedUrlPolicy(typeof (TModel)));
        }

        public ChainedBehaviorExpression Route(string pattern)
        {
            var expression = new ExplicitRouteConfiguration(pattern);
            _explicits.Add(expression);

            return expression.Chain();
        }

        public ChainedBehaviorExpression Route<T>(string pattern)
        {
            // TODO:  Throw exception in the chained expression if the input types
            // do not match
            var expression = new ExplicitRouteConfiguration<T>(pattern);
            _explicits.Add(expression);

            return expression.Chain();
        }

        public void Import<T>(string prefix) where T : FubuRegistry, new()
        {
            if (_imports.Any(x => x.Registry is T)) return;

            Import(new T(), prefix);
        }

        public void Modify<T>() where T : IRegistryModification, new()
        {
            new T().Modify(this);
        }

        public void Import(FubuRegistry registry, string prefix)
        {
            _imports.Add(new RegistryImport{
                Prefix = prefix,
                Registry = registry
            });
        }

        public void IncludeDiagnostics(bool shouldInclude)
        {
            if (shouldInclude)
            {
                UsingObserver(new RecordingConfigurationObserver());
                Import<DiagnosticsRegistry>(string.Empty);
                Modify<DiagnosticsPackage>();
                _systemPolicies.Add(new DiagnosticBehaviorPrepender());
            }
            else
            {
                Actions
                    .ExcludeTypes(t => t.HasAttribute<DiagnosticsActionAttribute>())
                    .ExcludeMethods(call => call.Method.HasAttribute<DiagnosticsActionAttribute>());
            }
        }

        public void RegisterPartials(Action<IPartialViewTypeRegistrationExpression> registration)
        {
            var expression = new PartialViewTypeRegistrationExpression(_partialViewTypes);
            registration(expression);
        }


        /// <summary>
        ///   This allows you to drop down to direct manipulation of the BehaviorGraph
        ///   produced by this FubuRegistry
        /// </summary>
        /// <param name = "alteration"></param>
        public void Configure(Action<BehaviorGraph> alteration)
        {
            addExplicit(alteration);
        }

        #region Nested type: RegistryImport

        public class RegistryImport
        {
            public string Prefix { get; set; }
            public FubuRegistry Registry { get; set; }

            public void ImportInto(BehaviorGraph graph)
            {
                graph.Import(Registry.BuildGraph(), Prefix);
            }
        }

        #endregion
    }
}