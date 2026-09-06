using System.Collections.Generic;
using System;
using UnityEditor.Experimental.GraphView;
using UnityEngine.UIElements;
using UnityEngine;

namespace CreatureTime.Editor.Graph
{

    /// Port to handle communication with our custom model.
    public class CtGraphEditorPort : Port
    {
        /// Referenced the original DefaultEdgeConnectorListener from Port with modifications to communicate with the model.
        private class DefaultEdgeConnectorListener : IEdgeConnectorListener
        {
            private GraphViewChange m_GraphViewChange;
            private List<Edge> m_EdgesToCreate;
            private List<GraphElement> m_EdgesToDelete;

            public DefaultEdgeConnectorListener()
            {
                m_EdgesToCreate = new List<Edge>();
                m_EdgesToDelete = new List<GraphElement>();
                m_GraphViewChange.edgesToCreate = m_EdgesToCreate;
            }

            public void OnDropOutsidePort(Edge edge, Vector2 position)
            {
            }

            public void OnDrop(UnityEditor.Experimental.GraphView.GraphView graphView, Edge edge)
            {
                m_EdgesToCreate.Clear();
                m_EdgesToCreate.Add(edge);
                m_EdgesToDelete.Clear();
                if (edge.input.capacity == Port.Capacity.Single)
                {
                    foreach (Edge connection in edge.input.connections)
                    {
                        if (connection != edge)
                            m_EdgesToDelete.Add((GraphElement)connection);
                    }
                }

                if (edge.output.capacity == Port.Capacity.Single)
                {
                    foreach (Edge connection in edge.output.connections)
                    {
                        if (connection != edge)
                            m_EdgesToDelete.Add((GraphElement)connection);
                    }
                }

                if (m_EdgesToDelete.Count > 0)
                    graphView.DeleteElements((IEnumerable<GraphElement>)m_EdgesToDelete);
                List<Edge> edgesToCreate = m_EdgesToCreate;
                if (graphView.graphViewChanged != null)
                    edgesToCreate = graphView.graphViewChanged(m_GraphViewChange).edgesToCreate;
                foreach (Edge edge1 in edgesToCreate)
                {
                    // graphView.AddElement((GraphElement) edge1);
                    edge.input.Connect(edge1);
                    // edge.output.Connect(edge1);
                }
            }
        }

        /// The graph node the port is attached. Needed for resolving edge connections.
        CtGraphEditorNode _node;

        protected CtGraphEditorPort(CtGraphEditorNode node, Orientation portOrientation, Direction portDirection,
            Capacity portCapacity, Type type) : base(portOrientation, portDirection, portCapacity, type)
        {
            _node = node;
        }

        #region MODEL METHODS

        public void CommitConnect(Edge edge)
        {
            base.Connect(edge);
        }

        public void CommitDisconnect(Edge edge)
        {
            base.Disconnect(edge);
        }

        #endregion

        #region OVERRIDES

        public override void Connect(Edge edge)
        {
            _node.Connect(edge.output.node, edge.output, edge.input.node, edge.input);
        }

        public override void Disconnect(Edge edge)
        {
            _node.Disconnect(edge.viewDataKey);
        }

        #endregion

        public static CtGraphEditorPort Create<TEdge>(
            CtGraphEditorNode node,
            Orientation orientation,
            Direction direction,
            Type type)
            where TEdge : Edge, new()
        {
            DefaultEdgeConnectorListener listener = new DefaultEdgeConnectorListener();
            CtGraphEditorPort ele = new CtGraphEditorPort(node, orientation, direction, Capacity.Multi, type)
            {
                m_EdgeConnector = new EdgeConnector<TEdge>(listener)
            };
            ele.AddManipulator(ele.m_EdgeConnector);
            return ele;
        }
    }
}