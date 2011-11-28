using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Cryptool.PluginBase.Editor;

namespace Cryptool.CrypWin.Helper
{
    public class EditorTypePanelManager
    {
        public class EditorTypePanelProperties
        {
            public bool ShowLogPanel { get; set; }
            public bool ShowSettingsPanel { get; set; }
            public bool ShowComponentPanel { get; set; }
            public bool ShowMaximized { get; set; }
        }

        private Dictionary<Type, EditorTypePanelProperties> _editorTypeToPanelPropertiesMap = new Dictionary<Type, EditorTypePanelProperties>();

        public EditorTypePanelProperties GetEditorTypePanelProperties(Type editorType)
        {
            if (!_editorTypeToPanelPropertiesMap.ContainsKey(editorType))
            {
                var editorSettings = editorType.GetEditorInfoAttribute();
                if (editorSettings == null)
                {
                    return null;
                }

                EditorTypePanelProperties prop = new EditorTypePanelProperties()
                                                     {
                                                         ShowComponentPanel = editorSettings.ShowComponentPanel,
                                                         ShowLogPanel = editorSettings.ShowLogPanel,
                                                         ShowSettingsPanel = editorSettings.ShowSettingsPanel,
                                                         ShowMaximized = false
                                                     };
                _editorTypeToPanelPropertiesMap.Add(editorType, prop);
            }

            return _editorTypeToPanelPropertiesMap[editorType];
        }

        public void SetEditorTypePanelProperties(Type editorType, EditorTypePanelProperties panelProperties)
        {
            if (!_editorTypeToPanelPropertiesMap.ContainsKey(editorType))
            {
                _editorTypeToPanelPropertiesMap.Add(editorType, panelProperties);
            }
            else
            {
                _editorTypeToPanelPropertiesMap[editorType] = panelProperties;
            }
        }
    }
}
