/* Copyright (c) 2017 ExT (V.Sigalkin) */

using UnityEngine;

using System;
using System.Collections.Generic;

using extOpenNodes.Core;

namespace extOpenNodes
{
    [ExecuteInEditMode]
    public class ONWorkflow : MonoBehaviour
    {
        #region Extensions

        private class Epoch
        {
            public readonly List<ONElement> Elements = new List<ONElement>();
        }

        #endregion

        #region Public Vars

        public GameObject NodesRoot
        {
            get
            {
                if (_nodesRoot == null)
                {
                    _nodesRoot = new GameObject("NO Workflow Nodes");
                    _nodesRoot.transform.parent = transform;
                    _nodesRoot.hideFlags = HideFlags.HideInHierarchy;
                }

                return _nodesRoot;
            }
        }

#if UNITY_EDITOR
        // EDITOR ONLY
        public Vector2 EnvironmentPosition
        {
            get { return _enviromentPositon; }
            set { _enviromentPositon = value;}
        }
#endif

        #endregion

        #region Private Vars

        private readonly Dictionary<string, ONElement> _elementsDictionary = new Dictionary<string, ONElement>();

        [SerializeField]
        private List<ONNode> _nodes = new List<ONNode>();

        [SerializeField]
        private List<ONLink> _links = new List<ONLink>();

#if UNITY_EDITOR
        // EDITOR ONLY
        [SerializeField]
        private Vector2 _enviromentPositon;
#endif

        [SerializeField]
        private GameObject _nodesRoot;

        private Epoch[] _epoches = new Epoch[0];

        #endregion

        #region Unity Methods

        protected void Awake()
        {
            Register();
        }

        protected void Update()
        {
            if (!Application.isPlaying)
                return;

            Process();
        }

        #endregion

        #region Public Methods

        public void Register()
        {
            _elementsDictionary.Clear();

            foreach (var node in _nodes)
            {
                RegisterElement(node);

                foreach (var property in node.InputProperties)
                {
                    RegisterElement(property);
                }

                foreach (var property in node.OutputProperties)
                {
                    RegisterElement(property);
                }
            }

            foreach (var link in _links)
            {
                RegisterElement(link);

                link.Connect();
            }

            Rebuild();
        }

        public void Process()
        {
            for (var i = 0; i < _epoches.Length; i++)
            {
                var epoch = _epoches[i];

                foreach (var element in epoch.Elements)
                {
                    element.Process();
                }
            }
        }

        public ONElement GetElement(string elementId)
        {
            if (_elementsDictionary.ContainsKey(elementId))
                return _elementsDictionary[elementId];

            return null;
        }

        public ONNode CreateNode(Component targetComponent)
        {
            var node = Create<ONNode>();
            node.Target = targetComponent;

            _nodes.Add(node);

            if (_epoches.Length == 0)
                _epoches = new Epoch[] { new Epoch() };

            _epoches[0].Elements.Add(node);

            return node;
        }

        public ONLink CreateLink(ONProperty sourceProperty, ONProperty targetProperty)
        {
            var link = Create<ONLink>();
            link.SourceProperty = sourceProperty;
            link.TargetProperty = targetProperty;
            link.Connect();

            _links.Add(link);

            Rebuild();

            return link;
        }

        public ONProperty CreateProperty(ONNode node, ONPropertyType propertyType)
        {
            var property = Create<ONProperty>();
            property.PropertyType = propertyType;
            property.Node = node;

            if (propertyType == ONPropertyType.Input)
                node.InputProperties.Add(property);
            if (propertyType == ONPropertyType.Output)
                node.OutputProperties.Add(property);

            return property;
        }

        public void RemoveNode(ONNode node)
        {
            if (!_nodes.Contains(node))
                return;

            var links = GetElementLinks(node);
            foreach (var link in links)
            {
                RemoveLink(link);
            }

            var properties = GetNodeProperties(node);
            foreach (var property in properties)
            {
                RemoveProperty(property);
            }

            RemoveEpochElement(node);

            _nodes.Remove(node);
            _elementsDictionary.Remove(node.ElementId);
        }

        public void RemoveLink(ONLink link)
        {
            if (!_links.Contains(link))
                return;

            link.Unconnect();

            RemoveEpochElement(link);

            _links.Remove(link);
            _elementsDictionary.Remove(link.ElementId);
        }

        public void RemoveProperty(ONProperty property)
        {
            if (!_elementsDictionary.ContainsKey(property.ElementId))
                return;

            var node = property.Node;

            if (node.InputProperties.Contains(property))
                node.InputProperties.Remove(property);
            if (node.OutputProperties.Contains(property))
                node.OutputProperties.Remove(property);

            _elementsDictionary.Remove(property.ElementId);
        }

        public List<ONProperty> GetNodeProperties(ONNode node)
        {
            var properties = new List<ONProperty>();
            properties.AddRange(node.InputProperties);
            properties.AddRange(node.OutputProperties);

            return properties;
        }

        public List<ONLink> GetElementLinks(ONElement element)
        {
            var links = new List<ONLink>();

            foreach (var link in _links)
            {
                if (link.SourceProperty.Node == element ||
                    link.TargetProperty.Node == element)
                {
                    links.Add(link);
                    continue;
                }

                if (link.SourceProperty == element ||
                    link.TargetProperty == element)
                {
                    links.Add(link);
                }
            }

            return links;
        }

        public ONNode[] GetNodes()
        {
            var nodes = new ONNode[_nodes.Count];
            _nodes.CopyTo(nodes);

            return nodes;
        }

        public ONLink[] GetLinks()
        {
            var links = new ONLink[_links.Count];
            _links.CopyTo(links);

            return links;
        }

        #endregion

        #region Private Methods

        private void Rebuild()
        {
            var nodes = GetNodes();
            foreach (var node in nodes)
            {
                node.EpochId = 0;

                if (node.Target == null)
                {
                    var nodeLinks = GetElementLinks(node);
                    foreach (var link in nodeLinks)
                    {
                        RemoveLink(link);
                    }

                    var nodeProperties = GetNodeProperties(node);
                    foreach (var property in nodeProperties)
                    {
                        RemoveProperty(property);
                    }
                }
            }

            var links = GetLinks();
            foreach (var link in links)
            {
                link.EpochId = 0;

                if (link.TargetProperty == null || link.SourceProperty == null || 
                    link.TargetProperty.Node == null || link.SourceProperty.Node == null)
                {
                    RemoveLink(link);
                }
            }

            var rootNodes = GetRootNodes();

            foreach (var node in rootNodes)
                ProcessNode(node, 0);

            var epochsDictionary = new Dictionary<int, Epoch>();

            foreach (var node in _nodes)
            {
                if (!epochsDictionary.ContainsKey(node.EpochId))
                    epochsDictionary.Add(node.EpochId, new Epoch());

                epochsDictionary[node.EpochId].Elements.Add(node);
            }

            foreach (var link in _links)
            {
                if (!epochsDictionary.ContainsKey(link.EpochId))
                    epochsDictionary.Add(link.EpochId, new Epoch());

                epochsDictionary[link.EpochId].Elements.Add(link);
            }

            _epoches = new Epoch[epochsDictionary.Count];

            for (var index = 0; index < _epoches.Length; index++)
                _epoches[index] = epochsDictionary[index];
        }

        private List<ONNode> GetRootNodes()
        {
            var nodes = new List<ONNode>();

            foreach (var node in _nodes)
            {
                if (node.InputProperties.Count == 0)
                {
                    nodes.Add(node);
                    continue;
                }

                var links = GetElementLinks(node);
                if (links.Count == 0)
                {
                    nodes.Add(node);
                    continue;
                }

                var isValid = true;

                foreach (var link in links)
                {
                    if (link.TargetProperty.Node == node)
                        isValid = false;
                }

                if (isValid)
                    nodes.Add(node);
            }

            return nodes;
        }

        private List<ONLink> GetOutputLinks(ONNode node)
        {
            var links = new List<ONLink>();

            foreach (var link in _links)
            {
                foreach (var property in node.OutputProperties)
                {
                    if (link.SourceProperty == property)
                    {
                        links.Add(link);
                        break;
                    }
                }
            }

            return links;
        }

        private void ProcessNode(ONNode node, int epochId)
        {
            if (node.EpochId <= epochId)
                node.EpochId = epochId;
            else
                epochId = node.EpochId;

            epochId++;

            var linkEpochId = epochId;
            var nextEpochId = epochId + 1;

            var links = GetOutputLinks(node);

            foreach (var link in links)
            {
                link.EpochId = linkEpochId;

                ProcessNode(link.TargetProperty.Node, nextEpochId);
            }
        }

        private Epoch GetEpoch(int epochId)
        {
            if (0 > epochId || epochId >= _epoches.Length)
                return null;

            return _epoches[epochId];
        }

        private void RemoveEpochElement(ONElement element)
        {
            var epoch = GetEpoch(element.EpochId);
            if (epoch == null) return;

            if (epoch.Elements.Contains(element))
                epoch.Elements.Remove(element);
        }

        private void RegisterElement(ONElement element)
        {
            if (_elementsDictionary.ContainsKey(element.ElementId))
                return;

            element.Workflow = this;
            
            _elementsDictionary.Add(element.ElementId, element);
        }

        private T Create<T>() where T : ONElement
        {
            var guid = CreateGUID();
            var element = (T)Activator.CreateInstance(typeof(T));
            element.Workflow = this;
            element.ElementId = guid;

            _elementsDictionary.Add(guid, element);

            return element;
        }

        //TODO: Maybe replace?
        private string CreateGUID()
        {
            var guid = Guid.NewGuid().ToString("N");
            var isAvaible = !_elementsDictionary.ContainsKey(guid);

            while (!isAvaible)
            {
                guid = Guid.NewGuid().ToString("N");
                isAvaible = !_elementsDictionary.ContainsKey(guid);
            }

            return guid;
        }

        #endregion
    }
}
