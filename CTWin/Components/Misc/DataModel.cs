using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace CTWin.Components.Misc
{
    public class DataModel : Model
    {
        public MainWindow Parent { get; set; }
        public List<ChildDataModel> ChildDataModelList { get; private set; }

        public DataModel(MainWindow parent)
        {
            this.Parent = parent;
            this.ChildDataModelList = new List<ChildDataModel>();
        }

        public ChildDataModel GetPluginInfoModel()
        {
            ChildDataModel childModel = new ChildDataModel(Parent);
            ChildDataModelList.Add(childModel);
            return childModel;
        }
    }

    public class ChildDataModel : Model
    {
        public MainWindow Parent { get; set; }

        public ChildDataModel(MainWindow parent)
        {
            this.Parent = parent;
        }
    }

    public interface Model
    {
        MainWindow Parent { get; set; }
    }
}
