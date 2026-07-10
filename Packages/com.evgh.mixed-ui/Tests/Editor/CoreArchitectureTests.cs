using System;
using System.Linq;
using NUnit.Framework;
using UnityEditor;
using UnityEngine;
using UnityEngine.UI;

namespace Evgh.MixedUI.Tests
{
    public sealed class ProbeContextualElement : ContextualUIElement<TestConfigurableTarget>
    {
        public int BindCount { get; private set; }
        public int UnbindCount { get; private set; }

        protected override void OnThemeApplied(UITheme theme) { }

        protected override void OnBind(UICreationContext<TestConfigurableTarget> context)
        {
            BindCount++;
            context.Target.Invalidated += HandleInvalidated;
        }

        protected override void OnUnbind()
        {
            UnbindCount++;
            Target.Invalidated -= HandleInvalidated;
        }

        private void HandleInvalidated() => OnTargetBecameInvalid();
    }

    public sealed class TestConfigurableTarget : IConfigurableObject, IUITargetLifetime
    {
        private Action _invalidated;

        public string DisplayName { get; set; } = "Target";
        public bool IsVisible { get; set; } = true;
        public float Scale { get; set; } = 1f;
        public bool CanDelete { get; set; } = true;
        public bool IsValid { get; private set; } = true;
        public bool WasDeleted { get; private set; }
        public int SubscriptionCount { get; private set; }

        public event Action Invalidated
        {
            add { _invalidated += value; SubscriptionCount++; }
            remove { _invalidated -= value; SubscriptionCount--; }
        }

        public void Delete()
        {
            WasDeleted = true;
            Invalidate();
        }

        public void Invalidate()
        {
            if (!IsValid) return;
            IsValid = false;
            _invalidated?.Invoke();
        }
    }

    public sealed class CoreArchitectureTests
    {
        private const string ThemePath = "Packages/com.evgh.mixed-ui/Runtime/Assets/Themes/DefaultUITheme.asset";
        private const string CatalogPath = "Packages/com.evgh.mixed-ui/Runtime/Assets/DefaultUIPrefabCatalog.asset";

        private GameObject _root;
        private GameObject _viewer;

        [SetUp]
        public void SetUp()
        {
            _root = new GameObject("Test UI Root");
            _viewer = new GameObject("Test Viewer", typeof(Camera));
        }

        [TearDown]
        public void TearDown()
        {
            if (_root != null) UnityEngine.Object.DestroyImmediate(_root);
            if (_viewer != null) UnityEngine.Object.DestroyImmediate(_viewer);
        }

        [Test]
        public void BindingStoresTargetAndRebindingUnsubscribesPreviousTarget()
        {
            var element = new GameObject("Probe", typeof(RectTransform)).AddComponent<ProbeContextualElement>();
            element.transform.SetParent(_root.transform);
            var context = CreateContext();
            var first = new TestConfigurableTarget();
            var second = new TestConfigurableTarget();

            element.Bind(new UICreationContext<TestConfigurableTarget>(context, first, Spatial()));
            Assert.That(element.Target, Is.SameAs(first));
            Assert.That(first.SubscriptionCount, Is.EqualTo(1));

            element.Bind(new UICreationContext<TestConfigurableTarget>(context, second, Spatial()));
            Assert.That(element.Target, Is.SameAs(second));
            Assert.That(first.SubscriptionCount, Is.Zero);
            Assert.That(second.SubscriptionCount, Is.EqualTo(1));
            Assert.That(element.UnbindCount, Is.EqualTo(1));

            element.Unbind();
            Assert.That(second.SubscriptionCount, Is.Zero);
            Assert.That(element.IsBound, Is.False);
        }

        [Test]
        public void CommandUsesCorrectTargetAndHonorsCanExecute()
        {
            var target = new TestConfigurableTarget { CanDelete = false };
            var command = new UICommand<TestConfigurableTarget>(target, value => value.Delete(), value => value.CanDelete);
            var refreshes = 0;
            command.CanExecuteChanged += () => refreshes++;

            command.Execute();
            Assert.That(target.WasDeleted, Is.False);
            target.CanDelete = true;
            command.NotifyCanExecuteChanged();
            command.Execute();

            Assert.That(target.WasDeleted, Is.True);
            Assert.That(refreshes, Is.EqualTo(1));
        }

        [Test]
        public void FactoryAppliesThemeAndReportsMissingPrefab()
        {
            var theme = Load<UITheme>(ThemePath);
            var catalog = Load<UIPrefabCatalog>(CatalogPath);
            var factory = new UIFactory(catalog, theme);
            var label = factory.CreateLabel(_root.transform);
            Assert.That(label.Theme, Is.SameAs(theme));

            var emptyCatalog = ScriptableObject.CreateInstance<UIPrefabCatalog>();
            var emptyFactory = new UIFactory(emptyCatalog, theme);
            var error = Assert.Throws<InvalidOperationException>(() => emptyFactory.CreateLabel(_root.transform));
            Assert.That(error.Message, Does.Contain("label prefab"));
            UnityEngine.Object.DestroyImmediate(emptyCatalog);
        }

        [Test]
        public void ToggleSetValueWithoutNotifyDoesNotRaiseWrapperEvent()
        {
            var theme = Load<UITheme>(ThemePath);
            var catalog = Load<UIPrefabCatalog>(CatalogPath);
            var toggle = new UIFactory(catalog, theme).CreateToggle(_root.transform);
            var changes = 0;
            toggle.ValueChanged += _ => changes++;

            toggle.SetValueWithoutNotify(true);

            Assert.That(toggle.Value, Is.True);
            Assert.That(changes, Is.Zero);
        }

        [Test]
        public void BuilderPreservesTargetAndInvalidationReleasesMenu()
        {
            var target = new TestConfigurableTarget();
            var composer = new UIComposer(CreateContext());
            var handle = composer
                .For<IConfigurableObject>(target)
                .AnchoredTo(_root.transform)
                .WithLocalOffset(new Vector3(0.15f, 0.1f, 0f))
                .FaceUser()
                .ConfigurationPanel("Object Settings")
                .ShowVisibilityControl()
                .ShowScaleControl(0.1f, 3f)
                .ShowDeleteAction()
                .Build(_root.transform);

            Assert.That(handle.Instance.Target, Is.SameAs(target));
            target.Invalidate();
            Assert.That(handle.IsClosed, Is.True);
            Assert.That(target.SubscriptionCount, Is.Zero);
        }

        [Test]
        public void DeleteButtonUsesBoundTargetAndClosesImmediately()
        {
            var target = new TestConfigurableTarget();
            var handle = BuildMenu(target);
            var delete = handle.Instance.GetComponentsInChildren<UIButton>(true).Single(value => value.Text == "Delete");

            delete.Press();

            Assert.That(target.WasDeleted, Is.True);
            Assert.That(handle.IsClosed, Is.True);
        }

        [Test]
        public void BuilderIsSingleUseAndRejectsInvalidRange()
        {
            var target = new TestConfigurableTarget();
            Assert.Throws<ArgumentOutOfRangeException>(() => new UIComposer(CreateContext())
                .For(target).AnchoredTo(_root.transform).ConfigurationPanel("Settings").ShowScaleControl(1f, 1f));

            var builder = new UIComposer(CreateContext())
                .For(target)
                .AnchoredTo(_root.transform)
                .ConfigurationPanel("Settings");
            var handle = builder.Build(_root.transform);
            Assert.Throws<InvalidOperationException>(() => builder.Build(_root.transform));
            handle.Close();
        }

        [Test]
        public void SpatialContextUsesAnchorRatherThanHierarchyParent()
        {
            var target = new TestConfigurableTarget();
            var anchor = new GameObject("Separate Anchor");
            anchor.transform.position = new Vector3(2f, 3f, 4f);
            try
            {
                var handle = new UIComposer(CreateContext())
                    .For(target)
                    .AnchoredTo(anchor.transform)
                    .WithLocalOffset(Vector3.right)
                    .ConfigurationPanel("Settings")
                    .Build(_root.transform);

                Assert.That(handle.Instance.transform.parent, Is.EqualTo(_root.transform));
                Assert.That(Vector3.Distance(handle.Instance.transform.position, new Vector3(3f, 3f, 4f)), Is.LessThan(0.0001f));
                handle.Close();
            }
            finally { UnityEngine.Object.DestroyImmediate(anchor); }
        }

        private UIInstanceHandle<ObjectConfigurationMenu> BuildMenu(TestConfigurableTarget target) =>
            new UIComposer(CreateContext())
                .For<IConfigurableObject>(target)
                .AnchoredTo(_root.transform)
                .ConfigurationPanel("Settings")
                .ShowDeleteAction()
                .Build(_root.transform);

        private UIContext CreateContext()
        {
            var theme = Load<UITheme>(ThemePath);
            var catalog = Load<UIPrefabCatalog>(CatalogPath);
            var factory = new UIFactory(catalog, theme);
            return new UIContext(theme, factory, _viewer.transform);
        }

        private UISpatialContext Spatial() => new(_root.transform, Vector3.zero);

        private static T Load<T>(string path) where T : UnityEngine.Object
        {
            var value = AssetDatabase.LoadAssetAtPath<T>(path);
            Assert.That(value, Is.Not.Null, $"Missing generated asset: {path}");
            return value;
        }
    }
}
