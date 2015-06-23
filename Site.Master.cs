using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.UI;
using System.Web.UI.WebControls;
using AGI.Logger;

namespace RSMTool
{
    public partial class SiteMaster : System.Web.UI.MasterPage
    {
        public static int count;
        SiteMapNode[] existingNodes;
        SiteMapNode currentNode;

        protected void Page_Load(object sender, EventArgs e)
        {
            try
            {
                if (Session["nodes"] == null)
                {
                    count = 0;
                }
                if (Session["loadallNodes"] != null)
                {
                    SiteMapNode[] loadallNodes = (SiteMapNode[])Session["loadallNodes"];
                    loadallNodes = loadallNodes.Reverse().ToArray();
                    count = loadallNodes.Length;
                    Session["nodes"] = loadallNodes;
                    Session["loadallNodes"] = null;
                }
                SiteMap.CurrentNode.ReadOnly = false;
                currentNode = SiteMap.CurrentNode;
                SiteMapNode rootNode = SiteMap.RootNode;
                Stack<SiteMapNode> nodeStack = new Stack<SiteMapNode>();

                while (currentNode != rootNode)
                {
                    nodeStack.Push(currentNode);
                    currentNode = currentNode.ParentNode;
                }

                // If you want to include RootNode in your list
                nodeStack.Push(rootNode);
                existingNodes = nodeStack.ToArray();
                if (count > nodeStack.Count && Session["nodes"] != null)
                {
                    SiteMap.SiteMapResolve += SiteMapResolve;
                }
                else
                {
                    Session["nodes"] = nodeStack.ToArray(); ;
                    count = nodeStack.Count;
                }
            }
            catch (Exception ex)
            {
                Log.WriteToLog(ex.Message, ex.StackTrace);
            }
        }
       protected SiteMapNode SiteMapResolve(object sender,SiteMapResolveEventArgs e)
       {
           if (Session["removeaddedNodes"] == null && Session["nodes"]!=null)
           {
               SiteMapNode[] sessionNodes = (SiteMapNode[])Session["nodes"];
               sessionNodes = sessionNodes.Except(existingNodes).ToArray();
               if (sessionNodes.Length > 0)
               {
                   SiteMapNode newNode = default(SiteMapNode);
                   SiteMapNode currNode = SiteMap.CurrentNode;
                   for (int i = 0; i < sessionNodes.Length; i++)
                   {

                       newNode = sessionNodes[i];
                       if (i == 0)
                       {
                           newNode.ParentNode = currNode;
                       }
                       else
                       {
                           int j = i - 1;
                           newNode.ParentNode = sessionNodes[j];
                       }

                   }
                   SiteMap.SiteMapResolve -= SiteMapResolve;
                   return newNode;
               }
           }
           Session["removeaddedNodes"] = null;
           Session["nodes"] = existingNodes;
           count = existingNodes.Length;
           SiteMap.SiteMapResolve -= SiteMapResolve;
           return SiteMap.CurrentNode;
       }
    }
}
