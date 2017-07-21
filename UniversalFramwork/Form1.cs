using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Windows.Forms;

namespace UniversalFramwork
{
    public partial class Form1 : Form
    {
        const int initLabelX = 12;
        const int initLabelY = 17;
        const int incrementX = 80;
        const int incrementY = 30;
        const int initButtonX = 71;
        const int initButtonY = 12;
        const int initWindowX = 175;
        const int initWindowY = 83;
        const int Version = 1;

        struct functionDescription
        {
            public bool Windowed;
            public int SystemRequirement;
            public Dictionary<string, int> Requirement;
            public string GroupName;
            public string UtilName;
            public string ClassName;
            public int Version;
            public FileInfo ffi;
            public Assembly ass;
        }
        Dictionary<string, functionDescription> functionMap;
        public Form1()
        {
            refreshFunction();
        }
        public int refreshFunction()
        {
            InitializeComponent();
            functionMap = new Dictionary<string, functionDescription>();
            DirectoryInfo di = new DirectoryInfo(Application.StartupPath);
            FileInfo[] fi = di.GetFiles("*.dll");
            //int nextButtonLocationX = initButtonX+incrementX;
            //int nextButtonLocationY = initButtonY;
            int funcCount = 0;
            foreach (FileInfo ffi in fi)
            {
                Assembly ass = Assembly.LoadFile(Application.StartupPath + "/" + ffi.Name);
                Type tp = ass.GetType(ffi.Name.Substring(0, ffi.Name.IndexOf(@".dll")) + @".Description");
                if (tp == null)
                {
                    MessageBox.Show("Illegal component.File:" + ffi.Name+@":Cannot desolve a component which don't have a Description class","Loading Failiure");
                    continue;
                }
                functionDescription desStruct;
                Object obj = Activator.CreateInstance(tp);
                MethodInfo CN = tp.GetMethod("ClassName");
                string className = CN.Invoke(obj, null).ToString();
                //Check for replica class
                bool flag = false;
                foreach (KeyValuePair<string, functionDescription> kvp in functionMap)
                {
                    if (kvp.Value.ClassName.Equals(className))
                    {
                        MessageBox.Show("Invalid component.File:" + ffi.Name + " and file:"+kvp.Value.ffi.Name+":Duplicated class name:"+kvp.Key, "Loading Failiure");
                        flag = true;
                        continue;
                    }
                }
                if (flag)
                {
                    continue;
                }
                desStruct.ClassName = className;

                MethodInfo UN = tp.GetMethod("UtilName");
                string utilName = UN.Invoke(obj, null).ToString();
                desStruct.UtilName = utilName;

                MethodInfo Rq = tp.GetMethod("Requirement");
                Dictionary<string, int> requirement = (Dictionary<string, int>)Rq.Invoke(obj, null);
                desStruct.Requirement = requirement;

                MethodInfo SR = tp.GetMethod("SystemRequirement");
                int systemRequirement = (int)SR.Invoke(obj, null);
                desStruct.SystemRequirement = systemRequirement;
                if (systemRequirement > Version)
                {
                    MessageBox.Show("Invalid component.File:" + ffi.Name + "don't support current framework version", "Loading Failiure");
                    continue;
                }

                MethodInfo Wd = tp.GetMethod("Windowed");
                bool windowed = (bool)Wd.Invoke(obj, null);
                desStruct.Windowed = windowed;

                MethodInfo GN = tp.GetMethod("GroupName");
                string groupName = GN.Invoke(obj, null).ToString();
                desStruct.GroupName = groupName;

                MethodInfo Vn = tp.GetMethod("Version");
                int version = (int)Vn.Invoke(obj, null);
                desStruct.Version = version;

                desStruct.ffi = ffi;
                desStruct.ass = ass;

                functionMap.Add(className, desStruct);
            }
            if (requirementNotMet())
            {
                MessageBox.Show("Requirement not met.Several components will not be loaded", "Loading Failiure");
            }
            CreateButtons();
            return funcCount;
        }
        private void Newb_MouseClick(object sender, MouseEventArgs e)
        {
            Button btn = (Button)sender;
            Assembly ass = functionMap[btn.Name].ass;
            FileInfo ffi = functionMap[btn.Name].ffi;
            if (functionMap[btn.Name].Windowed)
            {
                Type ts = ass.GetType(ffi.Name.Substring(0, ffi.Name.IndexOf(@".dll")) + @".Form1", false);
                (Activator.CreateInstance(ts) as Form).Show();
            }
            else
            {
                Type tp = ass.GetType(ffi.Name.Substring(0, ffi.Name.IndexOf(@".dll")) + @"."+ functionMap[btn.Name].ClassName);
                object obj = Activator.CreateInstance(tp);
            }
        }

        
        private bool requirementNotMet()
        {
            string unmetRequirement = "";
            
            List<string> erase=new List<string>();
            foreach (KeyValuePair<string, functionDescription> kvp in functionMap)
            {
                functionDescription desStruct = kvp.Value;
                Dictionary<string, int> Requirement = desStruct.Requirement;
                foreach(KeyValuePair<string,int> rqp in Requirement)
                {
                    string reqName = rqp.Key;
                    int reqVern = rqp.Value;
                    try
                    {
                        int tys=functionMap[reqName].Version;
                    }
                    catch(KeyNotFoundException)
                    {
                        unmetRequirement += "Function:" + kvp.Key + ".Required function:" + reqName + " donot exist.\r\n";
                        erase.Add(kvp.Key);
                        continue;
                    }
                    if (reqVern > functionMap[reqName].Version)
                    {
                        unmetRequirement += "Function:" + kvp.Key + ".Required function:" + reqName + ".Required version:" + reqVern + ".Target version " + functionMap[reqName].Version + ".\r\n";
                        erase.Add(kvp.Key);
                    }
                }
            }
            if (erase.Count >= 0)
            {
                foreach (string st in erase)
                {
                    functionMap.Remove(st);
                }
            }
            if (unmetRequirement.Equals(""))
                return false;
            else
            {
                MessageBox.Show("Requirement(s) not met.Please record the following informations and contact the devoloper for tech support.\r\n" + unmetRequirement);
                return true;
            }
        }
        private int CreateButtons()
        {
            int totalCount=1;
            int groupTotalCount=0;
            Dictionary<string, SortedList<int, Point>> labelPointMap = new Dictionary<string, SortedList<int, Point>>();
            labelPointMap = addNewPoint(labelPointMap, "Internal");

            foreach (KeyValuePair<string, functionDescription> rqp in functionMap)
            {
                //NEW METHOD
                //===================================================================================================================
                string utilName = rqp.Value.UtilName;
                string className = rqp.Value.ClassName;
                string groupName = rqp.Value.GroupName;
                if (!utilName.Equals("Internal"))
                {
                    Button btn = new Button();
                    btn.Name = className;
                    btn.Text = utilName;
                    btn.Size = new Size(75, 23);
                    btn.Location = getNextPoint(labelPointMap, groupName);
                    labelPointMap = addNewPoint(labelPointMap, groupName);
                    if (btn.Location.X == 71)
                    {
                        Label lb = new Label();
                        lb.Text = groupName;
                        lb.Name = groupName;
                        lb.Location = new Point(12, 17 + incrementY * (labelPointMap.Keys.Count-1));
                        lb.Size = new Size(53, 12);
                        Controls.Add(lb);
                    }
                    Controls.Add(btn);

                }
                //===================================================================================================================
                //OLD METHOD
                //===================================================================================================================
                //int groupCount = 0;
                //string utilName = rqp.Value.UtilName;
                //string className = rqp.Value.ClassName;
                //string groupName = rqp.Value.GroupName;
                //if (!utilName.Equals("Internal"))
                //{
                //    Button newb = new Button();
                //    newb.Name = className;
                //    newb.Text = utilName;
                //    newb.Size = new Size(75, 23);
                //    int innerGroupCount = 0;
                //    int groupMaxX = 71;
                //    Label lbl;
                //    try
                //    {
                //        lbl = (Label)Controls.Find(groupName, false)[0];
                //    }
                //    catch (IndexOutOfRangeException)
                //    {
                //        MS("新label"+groupName);
                //        //first element in the group
                //        groupCount++;
                //        Label lb = new Label();
                //        lb.Text = groupName;
                //        lb.Name = groupName;
                //        lb.Location = new Point(12, 17 + incrementY * groupCount);
                //        Controls.Add(lb);
                //        //something
                //    }
                //    foreach (KeyValuePair<string, functionDescription> kvp in functionMap)
                //    {
                //        if (kvp.Value.GroupName.Equals(groupName))
                //        {
                //            innerGroupCount++;
                //            groupMaxX += incrementX;
                //        }
                //    }
                //    if (innerGroupCount > totalCount)
                //    {
                //        Size = new Size(Size.Width + incrementX, Size.Height);
                //    }
                //    newb.Location = new Point(71 + innerGroupCount * incrementX, 12 + totalCount * incrementY);
                //    newb.MouseClick += Newb_MouseClick;
                //    Controls.Add(newb);
                //}
            }
            Dictionary<int, int> ctd = getMaxWidth(labelPointMap);
            foreach (KeyValuePair<int, int> pv in ctd)
            {
                totalCount = pv.Key;
                groupTotalCount = pv.Value;
            }
            Size = new Size(175 + 80 * (totalCount), 83 + 30 * groupTotalCount);
            return 0;
        }
        private Dictionary<int,int> getMaxWidth(Dictionary<string, SortedList<int, Point>> pointMap)
        {

            //Dictionary<string, int> groupCountDictionary = new Dictionary<string, int>();
            int count = 0;
            int groups = 0;
            groups = pointMap.Keys.Count-1;
            foreach(SortedList<int, Point> pointStrip in pointMap.Values)
            {
                int ct = pointStrip.Count-1;
                if (ct > count)
                {
                    count = ct;
                }
            }
            //foreach (KeyValuePair<string, functionDescription> kvp in functionMap)
            //{
            //    if (groupCountDictionary.Keys.Contains(kvp.Value.GroupName))
            //    {
            //        groupCountDictionary[kvp.Value.GroupName] =groupCountDictionary[kvp.Value.GroupName] +1;
            //    }
            //    else
            //    {
            //        if (!kvp.Value.UtilName.Equals("Internal"))
            //        {
            //            groupCountDictionary.Add(kvp.Value.GroupName, 1);
            //            groups++;
            //        }
            //    }
            //}
            //foreach(KeyValuePair<string,int> kvpc in groupCountDictionary)
            //{
            //    count = kvpc.Value > count ? kvpc.Value : count;
            //}
            Dictionary<int, int> rtd = new Dictionary<int, int>();
            rtd.Add(count, groups);
            return rtd;
        }
        private void MS(string ss)
        {
            MessageBox.Show(ss);
        }
        private Dictionary<string,SortedList<int,Point>> addNewPoint(Dictionary<string, SortedList<int, Point>> pointMap,String newGroupName)
        {
            if (pointMap.Keys.Contains(newGroupName))
            {
                SortedList<int, Point> pointList = pointMap[newGroupName];
                pointList.Add(pointList.Keys.Count + 1, getNextPoint(pointMap, newGroupName));
                pointMap[newGroupName] = pointList;
                return pointMap;
            }
            else
            {
                SortedList<int, Point> pointList = new SortedList<int, Point>();
                pointList.Add(1, getNextPoint(pointMap, newGroupName));
                pointMap.Add(newGroupName, pointList);
                return pointMap;
            }
        }
        private Point getNextPoint(Dictionary<string, SortedList<int, Point>> pointMap, String nextGroupName)
        {
            if (pointMap.Keys.Contains(nextGroupName))
            {
                SortedList<int, Point> pointList = pointMap[nextGroupName];
                Point pt = pointMap[nextGroupName][pointList.Keys.Count];
                Point ptr = new Point(pt.X + 80, pt.Y);
                return ptr;
            }
            else
            {
                Point ptr = new Point(71, 12 + 30 * (pointMap.Keys.Count));
                return ptr;
            }
        }

        private void button1_Click(object sender, EventArgs e)
        {
            refreshFunction();
        }
    }
}
