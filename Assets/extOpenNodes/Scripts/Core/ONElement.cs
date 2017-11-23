/* Copyright (c) 2017 ExT (V.Sigalkin) */

using UnityEngine;

using System;

namespace extOpenNodes.Core
{
    [Serializable]
    public class ONElement
    {
        #region Public Vars

        public string ElementId
        {
            get { return _elementId; }
            set { _elementId = value; }
        }

        public ONWorkflow Workflow
        {
            get { return workflow; }
            set { workflow = value; }
        }

        public int EpochId
        {
            get { return _epochId; }
            set { _epochId = value; }
        }

        public string Name
        {
            get { return _name; }
            set { _name = value; }
        }

#if UNITY_EDITOR
        public virtual Vector2 ViewerPosition
        {
            get { return viewerRect.position; }
            set { viewerRect.position = value; }
        }

        public virtual Vector2 ViewerSize
        {
            get { return viewerRect.size; }
            set { viewerRect.size = value; }
        }
#endif

        #endregion

        #region Protected Vars

        [NonSerialized]
        protected ONWorkflow workflow; // Hack...

#if UNITY_EDITOR
        [SerializeField]
        protected Rect viewerRect;
#endif

        #endregion

        #region Private Vars

        [SerializeField]
        private string _elementId;

        [SerializeField]
        private string _name;

        private int _epochId;

        #endregion

        #region Public Methods

        public virtual void Process()
        { }

        #endregion

        #region Protected Methods

        #endregion

        #region Private Methods

        #endregion
    }
}