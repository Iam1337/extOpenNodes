/* Copyright (c) 2017 ExT (V.Sigalkin) */

using UnityEngine;

using System;

namespace extOpenNodes.Core
{
    [Serializable]
    public class ONLink : ONElement
    {
        #region Public Vars

        public ONProperty TargetProperty
        {
            get
            {
                if (_target == null && Workflow != null)
                {
                    _target = Workflow.GetElement(_targetId) as ONProperty;
                }

                return _target;
            }
            set
            {
                _target = value;
                _targetId = _target.ElementId;
            }
        }

        public ONProperty SourceProperty
        {
            get
            {
                if (_source == null && Workflow != null)
                {
                    _source = Workflow.GetElement(_sourceId) as ONProperty;
                }

                return _source;
            }
            set
            {
                _source = value;
                _sourceId = _source.ElementId;
            }
        }

        public ONLinkType LinkType
        {
            get { return _linkType; }
            set { _linkType = value; }
        }

        public bool IsConnected
        {
            get { return _connected; }
        }

        #endregion

        #region Private Vars

        [SerializeField]
        private string _sourceId;

        [SerializeField]
        private string _targetId;

        [SerializeField]
        private ONLinkType _linkType;

        [NonSerialized]
        private ONProperty _source;

        [NonSerialized]
        private ONProperty _target;

        [NonSerialized]
        private bool _connected;

        #endregion

        #region Public Methods

        public void Connect()
        {
            if (_connected)
                return;

            _connected = true;
        }

        public void Unconnect()
        {
            if (!_connected)
                return;

			if (TargetProperty != null && _linkType == ONLinkType.Value)
			{
				TargetProperty.SetValue(null);
			}

            _connected = false;
        }

        public override void Process()
        {
            if (_connected && _linkType == ONLinkType.Value)
            {
                TargetProperty.SetValue(SourceProperty.GetValue());
            }
        }

        #endregion
    }
}