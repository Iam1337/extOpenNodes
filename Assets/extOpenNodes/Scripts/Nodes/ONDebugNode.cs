/* Copyright (c) 2017 ExT (V.Sigalkin) */

using UnityEngine;

using extOpenNodes.Core;

namespace extOpenNodes.Nodes
{
    [ONNode("Developing/Debug", true)]
    public class ONDebugNode : MonoBehaviour, IONNodeProcess
    {
        #region Public Vars

        [ONInput(0, "Value")]
        public object Value { set { this.value = value; } }

        #endregion

        #region Protected Vars

        protected object value;

        [SerializeField]
        protected bool debug;

        #endregion

        #region Public Methods

        public void NodeProcess(ONNode node)
        {
            if (value != null && debug)
                Debug.Log("Value: " + value);

            value = null;
        }

        #endregion
    }
}