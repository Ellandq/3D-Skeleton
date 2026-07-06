using Model.Data.Registry;
using UnityEngine;

namespace Managers
{
    public class SaveManager : ManagerBase<SaveManager>
    {
        [Header("Hooks")]
        private DiffRegistry registry;
        public static DiffRegistry Registry => Instance.registry;
        

        protected override void Awake()
        {
            base.Awake();

            registry = new DiffRegistry();
        }
    }
}