using LightInject;

namespace WallpaperCore;

public static class IoCContainer
{
    private static readonly ServiceContainer Container = new();

    public static T Resolve<T>()
    {
        return Container.GetInstance<T>();
    }

    public static void Register<TService, TImplementation>() where TImplementation : TService
    {
        Container.Register<TService, TImplementation>();
    }
}