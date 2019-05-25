using Assets.Models;
using Assets.Models.Global_State;
using Assets.Models.Interfaces;
using Zenject;

public class DefaultInstaller : MonoInstaller
{
    public override void InstallBindings()
    {
        Container.Bind<IAtlas>().To<Atlas>().AsSingle();
        Container.Bind<IGlobal>().To<Global>().AsSingle();
    }
}