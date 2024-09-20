using SavageWorld.Runtime.Entities.NPC;
using System;
using UnityEngine;

namespace SavageWorld.Runtime.Utilities.FSM.Conditions
{
    [Serializable]
    public class WaitCondition : FSMConditionBase
    {
        #region Fields
        [SerializeField]
        private float _timeToWait;
        private float _time;
        #endregion

        #region Properties

        #endregion

        #region Events / Delegates

        #endregion

        #region Public Methods
        public override bool Check(NPCBase entity)
        {
            _time += Time.fixedDeltaTime;
            if (_time >= _timeToWait)
            {
                _time = 0f;
                return true;
            }
            return false;
        }
        #endregion

        #region Private Methods

        #endregion
    }
}