// Copyright (c) 2012-2021 Wojciech Figat. All rights reserved.

using System;
using System.Collections.Generic;
using System.Linq;
using FlaxEditor.GUI;
using FlaxEditor.GUI.Tabs;
using FlaxEngine;
using FlaxEngine.GUI;

namespace FlaxEditor.Windows
{
    /// <summary>
    /// Editor tool window for plugins management using <see cref="PluginManager"/>.
    /// </summary>
    /// <seealso cref="FlaxEditor.Windows.EditorWindow" />
    public sealed class PluginsWindow : EditorWindow
    {
        private Tabs _tabsPlugins;
        private Tabs _tabsPluginsCategory;
        
        private readonly List<CategoryEntry> _categories = new List<CategoryEntry>();
        private readonly Dictionary<Plugin, PluginEntry> _entries = new Dictionary<Plugin, PluginEntry>();

        /// <summary>
        /// Plugin entry control.
        /// </summary>
        /// <seealso cref="FlaxEngine.GUI.ContainerControl" />
        public class PluginEntry : ContainerControl
        {
            /// <summary>
            /// The plugin.
            /// </summary>
            public readonly Plugin Plugin;

            /// <summary>
            /// The category.
            /// </summary>
            public readonly CategoryEntry Category;

            /// <summary>
            /// Initializes a new instance of the <see cref="PluginEntry"/> class.
            /// </summary>
            /// <param name="plugin">The plugin.</param>
            /// <param name="category">The category.</param>
            /// <param name="desc">Plugin description</param>
            public PluginEntry(Plugin plugin, CategoryEntry category, ref PluginDescription desc)
            {
                Plugin = plugin;
                Category = category;

                float margin = 4;
                float iconSize = 64;

                var iconImage = new Image
                {
                    Brush = new SpriteBrush(Editor.Instance.Icons.Plugin128),
                    Parent = this,
                    Bounds = new Rectangle(margin, margin, iconSize, iconSize),
                };

                var icon = PluginUtils.TryGetPluginIcon(plugin);
                if (icon)
                    iconImage.Brush = new TextureBrush(icon);
                
              
                Size = new Vector2(300, 100);

                float tmp1 = iconImage.Right + margin;
                var nameLabel = new Label
                {
                    HorizontalAlignment = TextAlignment.Near,
                    AnchorPreset = AnchorPresets.HorizontalStretchTop,
                    Text = desc.Name,
                    Font = new FontReference(Style.Current.FontLarge),
                    Parent = this,
                    Bounds = new Rectangle(tmp1, margin, Width - tmp1 - margin, 28),
                };

                tmp1 = nameLabel.Bottom + margin + 8;
                var descLabel = new Label
                {
                    HorizontalAlignment = TextAlignment.Near,
                    VerticalAlignment = TextAlignment.Near,
                    Wrapping = TextWrapping.WrapWords,
                    AnchorPreset = AnchorPresets.HorizontalStretchTop,
                    Text = desc.Description,
                    Parent = this,
                    Bounds = new Rectangle(nameLabel.X, tmp1, nameLabel.Width, Height - tmp1 - margin),
                };

                string versionString = string.Empty;
                if (desc.IsAlpha)
                    versionString = "ALPHA ";
                else if (desc.IsBeta)
                    versionString = "BETA ";
                versionString += "Version ";
                versionString += desc.Version != null ? desc.Version.ToString() : "1.0";
                var versionLabel = new Label
                {
                    HorizontalAlignment = TextAlignment.Far,
                    VerticalAlignment = TextAlignment.Center,
                    AnchorPreset = AnchorPresets.TopRight,
                    Text = versionString,
                    Parent = this,
                    Bounds = new Rectangle(Width - 140 - margin, margin, 140, 14),
                };

                string url = null;
                if (!string.IsNullOrEmpty(desc.AuthorUrl))
                    url = desc.AuthorUrl;
                else if (!string.IsNullOrEmpty(desc.HomepageUrl))
                    url = desc.HomepageUrl;
                else if (!string.IsNullOrEmpty(desc.RepositoryUrl))
                    url = desc.RepositoryUrl;
                versionLabel.Font.Font.WaitForLoaded();
                var font = versionLabel.Font.GetFont();
                var authorWidth = font.MeasureText(desc.Author).X + 8;
                var authorLabel = new ClickableLabel
                {
                    HorizontalAlignment = TextAlignment.Far,
                    VerticalAlignment = TextAlignment.Center,
                    AnchorPreset = AnchorPresets.TopRight,
                    Text = desc.Author,
                    Parent = this,
                    Bounds = new Rectangle(Width - authorWidth - margin, versionLabel.Bottom + margin, authorWidth, 14),
                };
                if (url != null)
                {
                    authorLabel.TextColorHighlighted = Style.Current.BackgroundSelected;
                    authorLabel.DoubleClick = () => Platform.OpenUrl(url);
                }
            }
        }

        /// <summary>
        /// Plugins category control.
        /// </summary>
        /// <seealso cref="Tab" />
        public class CategoryEntry : Tab
        {
            /// <summary>
            /// The panel for the plugin entries.
            /// </summary>
            public VerticalPanel Panel;

            /// <summary>
            /// Initializes a new instance of the <see cref="CategoryEntry"/> class.
            /// </summary>
            /// <param name="text">The text.</param>
            public CategoryEntry(string text)
            : base(text)
            {
                var scroll = new Panel(ScrollBars.Vertical)
                {
                    AnchorPreset = AnchorPresets.StretchAll,
                    Offsets = Margin.Zero,
                    Parent = this,
                };
                var panel = new VerticalPanel
                {
                    AnchorPreset = AnchorPresets.HorizontalStretchTop,
                    Offsets = Margin.Zero,
                    IsScrollable = true,
                    Parent = scroll,
                };
                Panel = panel;
            }
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="PluginsWindow"/> class.
        /// </summary>
        /// <param name="editor">The editor.</param>
        public PluginsWindow(Editor editor)
        : base(editor, true, ScrollBars.None)
        {
            Title = "Plugins and Project References";
            
            // Create main Tabs
            _tabsPlugins = new Tabs
            {
                Orientation = Orientation.Horizontal,
                AnchorPreset = AnchorPresets.StretchAll,
                Offsets = Margin.Zero,
                TabsSize = new Vector2(120, 32),
                Parent = this
            };

            var tabPlugins = new Tab("Plugins");
            _tabsPlugins.AddTab(tabPlugins);
            _tabsPlugins.AddTab(new Tab("Project References"));
            
            // Create vertical Tabs for plugins category
            _tabsPluginsCategory = new Tabs
            {
                Orientation = Orientation.Vertical,
                AnchorPreset = AnchorPresets.StretchAll,
                Offsets = Margin.Zero,
                TabsSize = new Vector2(120, 32),
                Parent = tabPlugins
            };
            
            // Create default category for plugins
            var categoryPlugin = new CategoryEntry("No category")
            {
                AnchorPreset = AnchorPresets.StretchAll,
                Offsets = Margin.Zero,
                Parent = _tabsPluginsCategory
            };
            _categories.Add(categoryPlugin);

            OnPluginsChanged();
            PluginManager.PluginsChanged += OnPluginsChanged;
        }

        private void OnPluginsChanged()
        {
            List<PluginEntry> toRemove = null;
            foreach (var e in _entries)
            {
                if (!PluginManager.EditorPlugins.Contains(e.Key) && !PluginManager.GamePlugins.Contains(e.Key))
                {
                    if (toRemove == null)
                        toRemove = new List<PluginEntry>();
                    toRemove.Add(e.Value);
                }
            }
            if (toRemove != null)
            {
                foreach (var plugin in toRemove)
                {
                    Debug.Log(plugin.Plugin.Description.Category);
                    OnPluginRemove(plugin);
                }
            }
            foreach (var plugin in PluginManager.GamePlugins)
                OnPluginAdd(plugin);
            foreach (var plugin in PluginManager.EditorPlugins)
                OnPluginAdd(plugin);
        }

        private void OnPluginAdd(Plugin plugin)
        {
            var entry = GetPluginEntry(plugin);
            if (entry != null)
                return;

            // Special case for editor plugins (merge with game plugin if has linked)
            if (plugin is EditorPlugin editorPlugin && GetPluginEntry(editorPlugin.GamePluginType) != null)
                return;
            
            var desc = plugin.Description;
            CategoryEntry category;

            // Define the plugin category
            if (desc.Category == null)
            {
                category = _categories.Find(x => string.Equals(x.Text, "No category", StringComparison.OrdinalIgnoreCase));
                
                desc.Category = "No category";
            }
            else
                category = _categories.Find(x => string.Equals(x.Text, desc.Category, StringComparison.OrdinalIgnoreCase));
            if (category != null) 
                Debug.Log("je verifie la valeur de la categorie trouve = " + category.Text);
            // Create a new one if isn't exist in category list
            if (category == null)
            {
                category = new CategoryEntry(desc.Category)
                {
                    AnchorPreset = AnchorPresets.StretchAll,
                    Offsets = Margin.Zero,
                    Parent = _tabsPluginsCategory
                };
                _categories.Add(category);
                category.UnlockChildrenRecursive();
            }
            entry = new PluginEntry(plugin, category, ref desc)
            {
                Parent = category.Panel
            };
            
            _entries.Add(plugin, entry);
            foreach (var plug in _entries)
                Debug.Log("la categ " + plug.Value.Plugin.Description.Category);
            entry.UnlockChildrenRecursive();

            if (_tabsPlugins.SelectedTab == null)
                _tabsPlugins.SelectedTab = category;
        }

        private void OnPluginRemove(PluginEntry entry)
        {
            var category = entry.Category;
            _entries.Remove(entry.Plugin);
            entry.Dispose();
            Debug.Log("la categorie du plugin en train detre remove = " + entry.Plugin.Description.Category);
            // If category is not used anymore
            if (_entries.Values.Count(x => x.Category == category) == 0 && category.Text != "No category")
            {
                if (_tabsPlugins.SelectedTab == category)
                    _tabsPlugins.SelectedTab = null;

                category.Dispose();
                _categories.Remove(category);

                if (_tabsPlugins.SelectedTab == null && _categories.Count > 0)
                    _tabsPlugins.SelectedTab = _categories[0];
            }
        }

        /// <summary>
        /// Gets the plugin entry control.
        /// </summary>
        /// <param name="plugin">The plugin.</param>
        /// <returns>The entry.</returns>
        public PluginEntry GetPluginEntry(Plugin plugin)
        {
            _entries.TryGetValue(plugin, out var e);
            return e;
        }

        /// <summary>
        /// Gets the plugin entry control.
        /// </summary>
        /// <param name="pluginType">The plugin type.</param>
        /// <returns>The entry.</returns>
        public PluginEntry GetPluginEntry(Type pluginType)
        {
            if (pluginType == null)
                return null;

            foreach (var e in _entries.Keys)
            {
                if (e.GetType() == pluginType)
                    return _entries[e];
            }

            return null;
        }

        /// <inheritdoc />
        public override void OnDestroy()
        {
            PluginManager.PluginsChanged -= OnPluginsChanged;

            base.OnDestroy();
        }
    }
}
