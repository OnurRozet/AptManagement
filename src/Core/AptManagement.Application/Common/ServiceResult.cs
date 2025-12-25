using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AptManagement.Application.Common
{
    public class ServiceResult<T>
    {
        public T? ResultObject { get; set; }
        public bool IsSuccess { get; set; }
        public string Message { get; set; } = string.Empty;

        public static ServiceResult<T> Success(T result) => new() {ResultObject = result, IsSuccess = true ,Message=string.Empty};
        public static ServiceResult<T> Success(T result, string message) => new() {ResultObject = result, IsSuccess = true ,Message=message};

        public static ServiceResult<T> Error() => new() { IsSuccess = false, Message = string.Empty };
        public static ServiceResult<T> Error(string message) => new() {IsSuccess = false, Message = message };
        public static ServiceResult<T> Error(T result, string message) => new() { ResultObject = result, IsSuccess = false, Message = message };
    }
}
