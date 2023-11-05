//using AOSharp.Common.GameData;
//using AOSharp.Common.GameData.UI;
//using BehaviourTree;
//using Newtonsoft.Json;
//using SmokeLounge.AOtomation.Messaging.GameData;
//using System;
//using System.Collections.Generic;
//using System.IO;
//using System.Linq;

//namespace BTTemplate
//{
//    public class BtNodesView<TContext>
//    {
//        public BtStatusInfo<TContext> StatusInfo;
//        public View Root;
//        public List<BtNodeView<TContext>> Nodes;
//        private string _path;
//        private string _boldViewPath;
//        private string _smallViewPath;

//        public BtNodesView(string rootDir, string smallFileName, string boldFileName, View root, IBehaviour<TContext> behaviour)
//        {
//            StatusInfo = new BtStatusInfo<TContext>(behaviour);
//            Nodes = new List<BtNodeView<TContext>>();
//            Root = root;
//            _path = rootDir;
//            _smallViewPath = smallFileName;
//            _boldViewPath = boldFileName;
//        }

//        public void AddNode(string nodeName, NodeTextType nodeTextType, IBehaviour<TContext> node)
//        {
//            string viewPath = nodeTextType == NodeTextType.Bold ? _boldViewPath : _smallViewPath;

//            View btNodeView = View.CreateFromXml(_path + $"\\" + viewPath);

//            if (btNodeView.FindChild("BtText", out TextView btTextView))
//                btTextView.Text = nodeName;

//            Nodes.Add(new BtNodeView<TContext> { Node = node, Root = btNodeView, TextView = btTextView });
//            Root.AddChild(btNodeView, false);
//            Root.FitToContents();
//        }

//        public void AddNodesToRoot()
//        {
//            foreach (BtNodeView<TContext> node in Nodes)
//                Root.AddChild(node.Root, true);

//            Root.FitToContents();
//        }

//        public void RemoveNodesFromRoot()
//        {
//            foreach (BtNodeView<TContext> node in Nodes)
//                Root.RemoveChild(node.Root);

//            Root.FitToContents();
//            Nodes.Clear();
//        }

//        public class BtNodeView<TContext>
//        {
//            public View Root;
//            public TextView TextView;
//            public IBehaviour<TContext> Node;
//        }
//    }

//    public enum NodeTextType
//    {
//        Bold,
//        Small
//    }
//}