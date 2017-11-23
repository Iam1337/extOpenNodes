/* Copyright (c) 2017 ExT (V.Sigalkin) */

using UnityEngine;

using System;
using System.Collections.Generic;

namespace extOpenNodes.Core
{
    [Serializable]
    public class ONNode : ONElement
    {
        #region Public Vars

        public Component Target
        {
            get { return _target; }
            set { _target = value; }
        }

        public List<ONProperty> InputProperties
        {
            get { return _inputProperties; }
        }

        public List<ONProperty> OutputProperties
        {
            get { return _outputProperties; }
        }

        // ONLY EDITOR
        public bool CustomInspector
        {
            get { return _customInspector; }
            set { _customInspector = value; }
        }

        #endregion

        #region Private Vars

        [SerializeField]
        private List<ONProperty> _inputProperties = new List<ONProperty>();

        [SerializeField]
        private List<ONProperty> _outputProperties = new List<ONProperty>();

        [SerializeField]
        private Component _target;

        // ONLY EDITOR
        [SerializeField]
        private bool _customInspector;

        #endregion

        #region Public Methods

        public override void Process()
        {
            var targetInterface = _target as IONNodeProcess;

            if (targetInterface != null)
                targetInterface.NodeProcess(this);
        }

        #endregion
    }
}