using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using NLog.Config;
using SRHWiscMano.Core.Attributes;
using SRHWiscMano.Core.Helpers;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net.Http;
using System.Net;
using System.Reflection;
using System.Text;
using System.Text.Json.Serialization;
using System.Text.Json;
using System.Threading.Tasks;

namespace SRHWiscMano.Core.Helpers
{
    /// <summary>
    /// SRHWiscMano.Core.Attributes 의 속성을 갖는 클래스들을 Service에 등록하기 위한 Extenstion
    /// Service 특성에 따라 [Transient], [Singleton]를 지정하고, 이를 일괄적으로 Service에 등록하돌고 한다
    /// </summary>
    public static class AttributesServicesExtensions
    {
        private static Type[] GetExportedTypes(string assemblyName)
        {
            return AppDomain.CurrentDomain
                .GetAssemblies()
                .Where(a => a.FullName?.StartsWith(assemblyName) == true)
                .SelectMany(a => a.GetExportedTypes())
                .ToArray();
        }

        /// <summary>
        /// 입력된 AssemblyName을 기준으로 노출 가능한 types에서 Transient attribute 를 갖는 type의 클래스를 ServiceCollection 에 등록한다.
        /// </summary>
        /// <param name="assemblyName"></param>
        /// <param name="services"></param>
        public static void AddTransientTypes(this IServiceCollection services, string assemblyNames)
        {
            var exportedTypes = GetExportedTypes(assemblyNames);
            var transientTypes = exportedTypes
                .Select(
                    t => new {type = t, attributes = t.GetCustomAttributes(typeof(TransientAttribute), true)}
                )
                .Where(
                    t1 =>
                        t1.attributes is {Length: > 0}
                        && !t1.type.Name.Contains("Mock", StringComparison.OrdinalIgnoreCase)
                )
                .Select(t1 => new {Type = t1.type, Attribute = (TransientAttribute) t1.attributes[0]});

            foreach (var typePair in transientTypes)
            {
                if (typePair.Attribute.InterfaceType is null)
                {
                    services.AddTransient(typePair.Type);
                }
                else
                {
                    services.AddTransient(typePair.Attribute.InterfaceType, typePair.Type);
                }
            }
        }

        /// <summary>
        /// 입력된 AssemblyName을 기준으로 노출 가능한 types에서 Singleton attribute 를 갖는 type의 클래스를 ServiceCollection 에 등록한다.
        /// </summary>
        /// <param name="services"></param>
        /// <param name="assemblyNames"></param>
        public static void AddSingletonTypes(this IServiceCollection services, string assemblyNames)
        {
            var exportedTypes = GetExportedTypes(assemblyNames);

            var singletonTypes = exportedTypes
                .Select(
                    t => new {type = t, attributes = t.GetCustomAttributes(typeof(SingletonAttribute), true)}
                )
                .Where(
                    t1 =>
                        t1.attributes is {Length: > 0}
                        && !t1.type.Name.Contains("Mock", StringComparison.OrdinalIgnoreCase)
                )
                .Select(t1 => new {Type = t1.type, Attribute = (SingletonAttribute) t1.attributes[0]});

            foreach (var typePair in singletonTypes)
            {
                if (typePair.Attribute.InterfaceType is null)
                {
                    services.AddSingleton(typePair.Type);
                }
                else
                {
                    services.AddSingleton(typePair.Attribute.InterfaceType, typePair.Type);
                }
            }
        }
    }
}