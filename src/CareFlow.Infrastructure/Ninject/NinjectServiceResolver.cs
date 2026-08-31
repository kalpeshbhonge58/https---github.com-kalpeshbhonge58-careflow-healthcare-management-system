using CareFlow.Application.Interfaces;
using CareFlow.Infrastructure.Data;
using CareFlow.Infrastructure.Repositories;
using CareFlow.Infrastructure.Services;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Caching.Memory;
using Ninject;
using Ninject.Modules;

namespace CareFlow.Infrastructure.Ninject;

/// <summary>
/// Isolated Ninject DI demonstration for CareFlow Infrastructure services.
/// This is separate from the primary Microsoft.Extensions.DependencyInjection setup.
/// </summary>
public class CareFlowInfrastructureModule : NinjectModule
{
    private readonly string _connectionString;

    public CareFlowInfrastructureModule(string connectionString) =>
        _connectionString = connectionString;

    public override void Load()
    {
        Bind<ApplicationDbContext>().ToMethod(_ =>
        {
            var optionsBuilder = new DbContextOptionsBuilder<ApplicationDbContext>();
            optionsBuilder.UseSqlServer(_connectionString);
            return new ApplicationDbContext(optionsBuilder.Options);
        }).InTransientScope();

        Bind<IMemoryCache>().To<MemoryCache>().InSingletonScope()
            .WithConstructorArgument("optionsAccessor", _ => new MemoryCacheOptions());

        Bind<IPasswordHasher>().To<PasswordHasher>().InSingletonScope();
        Bind<ICacheService>().To<CacheService>().InSingletonScope();
        Bind<IUnitOfWork>().To<UnitOfWork>().InTransientScope();
        Bind(typeof(IRepository<>)).To(typeof(Repository<>)).InTransientScope();

        Bind<IPatientRepository>().To<PatientRepository>().InTransientScope();
        Bind<IDoctorRepository>().To<DoctorRepository>().InTransientScope();
        Bind<IAppointmentRepository>().To<AppointmentRepository>().InTransientScope();
        Bind<IDashboardRepository>().To<DashboardRepository>().InTransientScope();
        Bind<IAuditLogRepository>().To<AuditLogRepository>().InTransientScope();
        Bind<IMedicalRecordRepository>().To<MedicalRecordRepository>().InTransientScope();
        Bind<IPrescriptionRepository>().To<PrescriptionRepository>().InTransientScope();
        Bind<IInvoiceRepository>().To<InvoiceRepository>().InTransientScope();
        Bind<IMedicineRepository>().To<MedicineRepository>().InTransientScope();
        Bind<IUserRepository>().To<UserRepository>().InTransientScope();
        Bind<IDepartmentRepository>().To<DepartmentRepository>().InTransientScope();
    }
}

public class NinjectServiceResolver : IDisposable
{
    private readonly IKernel _kernel;

    public NinjectServiceResolver(string connectionString)
    {
        _kernel = new StandardKernel(new CareFlowInfrastructureModule(connectionString));
    }

    public T Resolve<T>() => _kernel.Get<T>();

    public void Dispose() => _kernel.Dispose();
}
