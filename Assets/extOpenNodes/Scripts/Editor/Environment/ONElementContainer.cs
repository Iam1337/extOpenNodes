/* Copyright (c) 2017 ExT (V.Sigalkin) */

using UnityEngine;

using extOpenNodes.Core;

namespace extOpenNodes.Editor.Environments
{
    public class ONElementContainer
    {
        #region Public Vars

        public ONWorkflowEnvironment Environment
        {
            get { return _environment; }
        }

        public ONElementContainer Parent
        {
            get { return _parent; }
            set { _parent = value; }
        }

        public virtual Vector2 Position
        {
            get
            {
                if (Parent != null)
                {
                    return LocalPosition + Parent.Position;
                }

                return LocalPosition + Environment.Position;
            }
            set
            {
                if (Parent != null)
                {
                    LocalPosition = value - Parent.Position;
                }
                else
                {
                    LocalPosition = value - Environment.Position;
                }
            }
        }

        public virtual Vector2 LocalPosition { get; set; }

        public virtual Vector2 Size { get; set; }

        public virtual Rect Rect
        {
            get { return new Rect(Position, Size); }
            set
            {
                Position = value.position;
                Size = value.size;
            }
        }

        public virtual Rect LocalRect
        {
            get { return new Rect(LocalPosition, Size); }
            set
            {
                LocalPosition = value.position;
                Size = value.size;
            }
        }


        #endregion

        #region Private Vars

        private ONElementContainer _parent;

        private ONWorkflowEnvironment _environment;

        #endregion

        #region Public Methods

        internal ONElementContainer(ONWorkflowEnvironment workflowEditor)
        {
            _environment = workflowEditor;
        }

        public virtual void Draw()
        { }

        #endregion

        #region Private Methods

        private ONElementContainer()
        { }

        #endregion
    }

    public class ONElementContainer<T> : ONElementContainer where T : ONElement
    {
        #region Public Vars

        public override Vector2 LocalPosition
        {
            get { return element.ViewerPosition; }
            set { element.ViewerPosition = value; }
        }

        public override Vector2 Size
        {
            get { return element.ViewerSize; }
            set { element.ViewerSize = value; }
        }

        #endregion

        #region Protected Vars

        protected T element;

        #endregion

        #region Public Methods

        public ONElementContainer(T element, ONWorkflowEnvironment workflowEditor) : base(workflowEditor)
        {
            this.element = element;
        }

        #endregion
    }
}