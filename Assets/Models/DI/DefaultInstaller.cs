using Assets.Models;
using Assets.Models.Interfaces;
using Zenject;

public class DefaultInstaller : MonoInstaller
{
    public override void InstallBindings()
    {
        Container.Bind<IAtlas>().To<Atlas>().AsSingle();
    }
}