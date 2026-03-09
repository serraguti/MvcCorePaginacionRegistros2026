using Microsoft.Data.SqlClient;
using Microsoft.EntityFrameworkCore;
using MvcCorePaginacionRegistros.Data;
using MvcCorePaginacionRegistros.Models;
using System.Diagnostics.Metrics;

#region VIEWS Y STORED PROCEDURES

/*
create view V_DEPARTAMENTOS_INDIVIDUAL
as
	select cast(
	ROW_NUMBER() over (order by DEPT_NO) as int) 
	as POSICION
	, DEPT_NO, DNOMBRE, LOC from DEPT
go

create procedure SP_GRUPO_DEPARTAMENTOS
(@posicion int)
as
	select DEPT_NO, DNOMBRE, LOC from V_DEPARTAMENTOS_INDIVIDUAL
	where POSICION >= @posicion and POSICION < (@posicion + 2)
go
exec SP_GRUPO_DEPARTAMENTOS 1

create view V_GRUPO_EMPLEADOS
as
	select cast(row_number() over (order by APELLIDO) as int)
	as POSICION, EMP_NO, APELLIDO, OFICIO, SALARIO, DEPT_NO
	from EMP
go
create procedure SP_GRUPO_EMPLEADOS
(@posicion int)
as
	select EMP_NO, APELLIDO, OFICIO, SALARIO, DEPT_NO 
	from V_GRUPO_EMPLEADOS
	where POSICION >= @posicion and POSICION < (@posicion + 3)
go
*/
#endregion

namespace MvcCorePaginacionRegistros.Repositories
{
    public class RepositoryHospital
    {
        private HospitalContext context;

        public RepositoryHospital(HospitalContext context)
        {
            this.context = context;
        }

        public async Task<int> GetNumeroRegistrosVistaDepartamentosAsync()
        {
            return await this.context.VistaDepartamentos.CountAsync();
        }

        public async Task<VistaDepartamento>
            GetVistaDepartamentoAsync(int posicion)
        {
            VistaDepartamento departamento =
                await this.context.VistaDepartamentos
                .Where(z => z.Posicion == posicion)
                .FirstOrDefaultAsync();
            return departamento;
        }

        public async Task<List<VistaDepartamento>>
            GetGrupoVistaDepartamentoAsync(int posicion)
        {
            //select * from V_DEPARTAMENTOS_INDIVIDUAL
            //where POSICION >= 1 and POSICION< (1 + 2)
            var consulta = from datos in this.context.VistaDepartamentos
                           where datos.Posicion >= posicion
                           && datos.Posicion < (posicion + 2)
                           select datos;
            return await consulta.ToListAsync();
        }

        public async Task<List<Departamento>>
            GetGrupoDepartamentosAsync(int posicion)
        {
            string sql = "SP_GRUPO_DEPARTAMENTOS @posicion";
            SqlParameter pamPosicion =
                new SqlParameter("@posicion", posicion);
            var consulta =
                this.context.Departamentos.FromSqlRaw(sql, pamPosicion);
            return await consulta.ToListAsync();
        }

        public async Task<int> GetEmpleadosCountAsync()
        {
            return await this.context.Empleados.CountAsync();
        }

        public async Task<List<Empleado>> 
            GetGrupoEmpleadosAsync(int posicion)
        {
            string sql = "SP_GRUPO_EMPLEADOS @posicion";
            SqlParameter pamPosicion =
                new SqlParameter("@posicion", posicion);
            var consulta =
                this.context.Empleados.FromSqlRaw(sql, pamPosicion);
            return await consulta.ToListAsync();
        }
    }
}
