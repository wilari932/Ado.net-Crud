using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Data.Sql;
using System.Data.SqlClient;
using System.Configuration;
using System.Text.RegularExpressions;
using System.IO;

namespace crud
{
    public class BaseEntity
    {
        [Indentity]
        public int Id { get; set; }
    }
    public class Person: BaseEntity
    {
       
        public string FirstName { get; set; }
        public string LastName { get; set; }
        public int? Age { get; set; }
    }
    class Program
    {
      
        public static string GetConnectionString(string key = "connectionstring")
        {
            try
            {
                return ConfigurationManager.ConnectionStrings[key].ConnectionString;
            }
            catch(Exception e)
            {
                //printo los posibles errores 
                Console.WriteLine("error 1: {0}, error 2:{1}",e.Message,e?.InnerException?.Message);
                // printo la linea de error
                Console.WriteLine( e.StackTrace);
                // voto el error 
                throw e;
            }
  
        }
        // static por que es Consola no otra app 
        private static SqlConnection con; 



        // crear base de datos
        public static void CreateDatabase()
        {
            // instanceo la conexion 
            using (con = new SqlConnection(GetConnectionString("Initconnectionstring")))
            {
                try
                {

                    string sqlQueryCreateDatabase = " Use master; " +
                        " CREATE DATABASE testDB; ";

                    SqlCommand sqlCommand = new SqlCommand(sqlQueryCreateDatabase,con);
                    con.Open();
                    sqlCommand.ExecuteNonQuery();
                    var sqlQueryCrateTable = " Use testDB; " +
                         "CREATE TABLE Person (" +
                         "Id int IDENTITY(1,1)" +
                         " PRIMARY KEY, LastName " +
                         "varchar(255) NOT NULL, " +
                         "FirstName varchar(255),Age int);";
                    sqlCommand.CommandText = sqlQueryCrateTable;
                    sqlCommand.ExecuteNonQuery();
                    //printo los posibles errores 
                    Console.WriteLine("Base de datos creada!");

                }
                catch(Exception e)
                {
                    //printo los posibles errores 
                    Console.WriteLine("error 1: {0}, error 2:{1}", e.Message, e?.InnerException?.Message);
                    // printo la linea de error
                    Console.WriteLine(e.StackTrace);
                    // voto el error 
                    throw e;
                }
            }

        }
        // necistas una clase que herede de BaseEntity para utilizar esta funcion 

        public static List<T> GetAll<T>() where T : BaseEntity, new()
        {
            using (con = new SqlConnection(GetConnectionString()))
            {
                try
                {
                    var list = new List<T>();
                    StringBuilder SqlQuery = new StringBuilder();
                    SqlQuery.Append("SELECT ");

                    var properties = typeof(T).GetProperties();

                    string parameters = string.Empty;
                    SqlCommand sqlCommand = new SqlCommand();
                    for (int i = 0; i < properties.Length; i++)
                    {
                        string delimiter = (i + 1 < properties.Length) ? "," : string.Empty;
                        parameters += $" {properties[i].Name} {delimiter} ";

                    }
                    SqlQuery.Append(parameters);
                    SqlQuery.Append(string.Format("FROM {0}", typeof(T).Name));

                    sqlCommand.CommandText = SqlQuery.ToString();
                    sqlCommand.Connection = con;
                    con.Open();
                  var read =   sqlCommand.ExecuteReader();
                    if (read.HasRows)
                    {
                        var prop = typeof(T).GetProperties();
                        while (read.Read())
                        {
                            var entity = new T();
                            for (int i = 0; i < prop.Length; i++)
                            {
                                var value = read[prop[i].Name];
                                prop[i].SetValue(entity, Convert.ChangeType(value, Nullable.GetUnderlyingType(prop[i].PropertyType) ?? prop[i].PropertyType));
                            }
                            list.Add(entity);
                        }
                        
                    }
                    return list;
                  

                }
                catch (Exception e)
                {
                    //printo los posibles errores 
                    Console.WriteLine("error 1: {0}, error 2:{1}", e.Message, e?.InnerException?.Message);
                    // printo la linea de error
                    Console.WriteLine(e.StackTrace);
                    // voto el error 
                    throw e;
                }



            }
        }

        public static T Update<T>(T entity) where T : BaseEntity
        {
            using (con = new SqlConnection(GetConnectionString()))
            {
                try
                {

                    StringBuilder SqlQuery = new StringBuilder();
                    SqlQuery.Append(string.Format(" UPDATE {0} SET", typeof(T).Name));
                   
                    var properties = typeof(T).GetProperties().Where(x=> !Attribute.IsDefined(x, typeof(IndentityAttribute))).ToArray();
                
                    string parameters = string.Empty;
                    SqlCommand sqlCommand = new SqlCommand();
                    for (int i = 0; i < properties.Length; i++)
                    {
          
                        string delimiter = (i + 1 < properties.Length) ? "," : string.Empty;
                        parameters += $" {properties[i].Name} = @{properties[i].Name}{delimiter} ";
                      
                        sqlCommand.Parameters.AddWithValue($"@{properties[i].Name}", properties[i].GetValue(entity));
                    }
                    SqlQuery.Append($"{parameters} WHERE {nameof(entity.Id)} = @{nameof(entity.Id)};");
                    sqlCommand.Parameters.AddWithValue($"@{nameof(entity.Id)}", entity.Id);


                    sqlCommand.CommandText = SqlQuery.ToString();
                    sqlCommand.Connection = con;
                    con.Open();
                    sqlCommand.ExecuteNonQuery();
                    Console.WriteLine("Persona Actualizada");

                }
                catch (Exception e)
                {
                    //printo los posibles errores 
                    Console.WriteLine("error 1: {0}, error 2:{1}", e.Message, e?.InnerException?.Message);
                    // printo la linea de error
                    Console.WriteLine(e.StackTrace);
                    // voto el error 
                    throw e;
                }

                return entity;

            }
        }
        public static void Delete<T>(T entity) where T : BaseEntity
        {
            using (con = new SqlConnection(GetConnectionString()))
            {
                try
                {
                    StringBuilder SqlQuery = new StringBuilder();
                    SqlQuery.Append(string.Format(" Delete FROM {0} ", typeof(T).Name));
                    
                    SqlCommand sqlCommand = new SqlCommand();
                
                    SqlQuery.Append($" WHERE {nameof(entity.Id)} = @{nameof(entity.Id)};");
                    sqlCommand.Parameters.AddWithValue($"@{nameof(entity.Id)}", entity.Id);
              
                    sqlCommand.CommandText = SqlQuery.ToString();
                    sqlCommand.Connection = con;
                    con.Open();
                    sqlCommand.ExecuteNonQuery();
                    Console.WriteLine("Persona Borrada");
                }
                catch (Exception e)
                {
                    //printo los posibles errores 
                    Console.WriteLine("error 1: {0}, error 2:{1}", e.Message, e?.InnerException?.Message);
                    // printo la linea de error
                    Console.WriteLine(e.StackTrace);
                    // voto el error 
                    throw e;
                }



            }
        }
        public static T Create<T>(T entity) where T : BaseEntity
        {
            using (con = new SqlConnection(GetConnectionString()))
            {
                try
                {

                    StringBuilder SqlQuery = new StringBuilder();
                    SqlQuery.Append(string.Format(" INSERT INTO {0} ", nameof(Person)));
                    SqlQuery.Append(" ( ");
                    var properties = typeof(T).GetProperties().Where(x => !Attribute.IsDefined(x, typeof(IndentityAttribute))).ToArray(); 
                    
                    string parameters = string.Empty;
                    string values = string.Empty;
                    SqlCommand sqlCommand = new SqlCommand();
                    for (int i = 0; i < properties.Length; i++)
                    {
                        string delimiter = (i + 1 < properties.Length) ? "," : string.Empty;
                        parameters += $" {properties[i].Name}{delimiter} ";
                        values += $" @{properties[i].Name}{delimiter} ";
                        sqlCommand.Parameters.AddWithValue($"@{properties[i].Name}", properties[i].GetValue(entity));
                    }
                    SqlQuery.Append($"{parameters}) ");
                    SqlQuery.Append($" VALUES({values}); ");
                    SqlQuery.Append(" SELECT SCOPE_IDENTITY() ");
                    sqlCommand.CommandText = SqlQuery.ToString();
                    sqlCommand.Connection = con;
                    con.Open();
                    entity.Id =  Convert.ToInt32(sqlCommand.ExecuteScalar());
                    Console.WriteLine("Persona Creada");

                }
                catch (Exception e)
                {
                    //printo los posibles errores 
                    Console.WriteLine("error 1: {0}, error 2:{1}", e.Message, e?.InnerException?.Message);
                    // printo la linea de error
                    Console.WriteLine(e.StackTrace);
                    // voto el error 
                    throw e;
                }

                return entity;

            }
        }
        static void Main(string[] args)
        {
            //utiliza este metodo solo una vez 
            //CreateDatabase();
          var person =   Create(new Person { FirstName = "test", LastName = "test", Age = 32, });
            // cambiamos el nombre
            person.FirstName = "Diego";
            // actualizo
            Update(person);
            // ver todo el registro 
            var Persons = GetAll<Person>();

            // borrrar 
            Delete(person);

            Console.ReadKey();
        }
    }
}
