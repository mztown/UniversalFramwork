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
        const int incrementX = 30;
        const int incrementY = 80;
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
            public int buttonX;
            public int buttonY;
            public FileInfo ffi;
            public Assembly ass;
        }
        Dictionary<string, functionDescription> functionMap;
        public Form1()
        {
            InitializeComponent();
            functionMap = new Dictionary<string, functionDescription>();
        }
        public int refreshFunction()
        {
            DirectoryInfo di = new DirectoryInfo(Application.StartupPath);
            FileInfo[] fi = di.GetFiles("*.dll");
            int nextButtonLocationX = initButtonX+incrementX;
            int nextButtonLocationY = initButtonY+incrementY;
            int groupCount = 1;
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
                if (!groupName.Equals("Internal"))
                {
                    groupCount++;
                }
                desStruct.GroupName = groupName;

                MethodInfo Vn = tp.GetMethod("Version");
                int version = (int)Vn.Invoke(obj, null);
                desStruct.Version = version;


                if (!utilName.Equals("Internal"))
                {
                    Button newb = new Button();
                    newb.Name = className;
                    newb.Text = utilName;
                    if (groupCount == 1)
                    {

                    }
                    newb.Size = new Size(75, 23);
                    newb.Location = new Point(nextButtonLocationX, nextButtonLocationY);
                    nextButtonLocationX = 81 + nextButtonLocationX;
                    newb.MouseClick += Newb_MouseClick;
                    this.Controls.Add(newb);
                }

                desStruct.ffi = ffi;
                desStruct.ass = ass;
                desStruct.buttonX = nextButtonLocationX;
                desStruct.buttonY = nextButtonLocationY;

                functionMap.Add(className, desStruct);
            }
            if (requirementNotMet())
            {
                MessageBox.Show("Requirement not met.Several components will not be loaded", "Loading Failiure");
            }
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
            foreach (KeyValuePair<string, functionDescription> kvp in functionMap)
            {
                functionDescription desStruct = kvp.Value;
                Dictionary<string, int> Requirement = desStruct.Requirement;
                foreach(KeyValuePair<string,int> rqp in Requirement)
                {
                    string reqName = rqp.Key;
                    int reqVern = rqp.Value;
                    if (reqVern > functionMap[reqName].Version)
                    {
                        unmetRequirement+="Function "+kvp.Key+" requires function "+reqName+" on version "+
                        
                    }
                }
            }
            if (unmetRequirement.Equals(""))
            {
                return false;
            }
            else
            {
                return true;
            }
        }
    }
}
