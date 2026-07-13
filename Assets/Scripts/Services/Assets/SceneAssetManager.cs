using Managers;
using Model.Data.Registry;

namespace Services.Assets
{
    public abstract class SceneAssetManager
    {
        protected readonly AssetManager assetManager;
        protected readonly AssetRegistry registry;
        protected readonly AssetFactory factory;

        protected SceneAssetManager(AssetManager assetManager, AssetRegistry registry, AssetFactory factory)
        {
            this.assetManager = assetManager;
            this.registry = registry;
            this.factory = factory;
        }
    }
}