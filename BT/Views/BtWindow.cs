//using AOSharp.Common.GameData;
//using AOSharp.Common.GameData.UI;
//using AOSharp.Common.Unmanaged.Interfaces;
//using AOSharp.Core;
//using AOSharp.Core.Inventory;
//using AOSharp.Core.UI;
//using BehaviourTree;
//using Newtonsoft.Json;
//using SmokeLounge.AOtomation.Messaging.GameData;
//using System;
//using System.Collections.Generic;
//using System.IO;
//using System.Linq;

//namespace BTTemplate
//{
//    public class BtWindow<TContext> : AOSharpWindow
//    {
//        private View _root;
//        public TextView ActiveNode;
//        public BtNodesView<TContext> BtNodes;

//        IBehaviour<TContext> _behaviour;
//        private string _rootDir;
//        private string _defaultNodeText;

//        public BtWindow(string name, IBehaviour<TContext> behaviourTree, string windowPath, WindowStyle windowStyle = WindowStyle.Popup, WindowFlags flags = WindowFlags.AutoScale | WindowFlags.NoFade) : base(name, windowPath, windowStyle, flags)
//        {
//            _defaultNodeText = name;
//            _rootDir = System.IO.Path.GetDirectoryName(windowPath);
//            _behaviour = behaviourTree;
//        }

//        protected override void OnWindowCreating()
//        {
//            try
//            {
//                Window.FindView("NodeRoot", out _root);
//                BtNodes = new BtNodesView<TContext>(_rootDir, "BtNodeViewSmall.xml", "BtNodeViewBold.xml", _root, _behaviour);

//                Window.FindView("ActiveNode", out ActiveNode);
//                ActiveNode.Text = _defaultNodeText;

//                Window.FindView("Icon", out BitmapView icon);
//                icon.SetBitmap("HeaderIcon");

//                Window.MoveTo(990, 300);
//            }
//            catch (Exception e)
//            {
//                Chat.WriteLine(e);
//            }
//        }

//        public void StatusUpdate()
//        {
//            int gfxId = 0;

//            if (BtNodes.Nodes.Count() == 0)
//                BtNodes.StatusInfo.Render(this);

//            BtNodes.AddNodesToRoot();
//            _root.FitToContents();

//            ActiveNode.Text = _defaultNodeText;
//        }

//        public void Update()
//        {
//            BtNodes.StatusInfo.Update(this);
//        }
//    }
//}