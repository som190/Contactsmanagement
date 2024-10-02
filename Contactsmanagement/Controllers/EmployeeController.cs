using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Caching.Memory;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json;

namespace Contactsmanagement.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class EmployeeController : ControllerBase
    {


        private readonly ILogger<EmployeeController> _logger;
        private readonly IMemoryCache _cache;
        string cacheKey = "Employees";
        public EmployeeController(ILogger<EmployeeController> logger, IMemoryCache cache)
        {
            _logger = logger;
            _cache = cache;
        }

        [HttpGet]
        public IActionResult Get()
        {
            CheckCache();
            var employees = _cache.Get(cacheKey) as List<Employee>;
            return Ok(employees);
        }
        [Route("{id}")]
        [HttpGet]
        public Employee Get(int id)
        {
            CheckCache();
            var employees = _cache.Get(cacheKey) as List<Employee>;
            if (employees != null)
            {
                var data = employees.Where(a => a.Id == id).FirstOrDefault();
                return data;
            }
            return null;

        }
        
        [HttpPost]
        
        public List<Employee> Add(Employee employee)
        {
            CheckCache();
            var employees = _cache.Get(cacheKey) as List<Employee>;
            employees.Add(employee);
            UpdateCache(employees);
            return employees;
        }
        [HttpPut]
        [Route("{id}")]
        public List<Employee> Update( int id,Employee employee)
        {
            CheckCache();
            var employees = _cache.Get(cacheKey) as List<Employee>;
            if (employees != null)
            {
                var data = employees.Where(a => a.Id == id).FirstOrDefault();
                employees.Remove(data);
                employees.Insert(id-1, employee);
            }

            UpdateCache(employees);
            return employees;


        }
        [HttpDelete]
        [Route("{id}")]
        public List<Employee> Delete(int id)
        {
            CheckCache();
            var employees = _cache.Get(cacheKey) as List<Employee>;
            if (employees != null)
            {
                var data = employees.Where(a => a.Id == id).FirstOrDefault();
                employees.Remove(data);               
            }

            UpdateCache(employees);
            return employees;


        }
        private void CheckCache()
        {
            if (!_cache.TryGetValue(cacheKey, out List<Employee>? employees))
            {
                SetCache();
            }
        }
        private void SetCache()
        {
            using (StreamReader r = new StreamReader(@"Data\Employee.json"))
            {
                string json = r.ReadToEnd();
                var items = JsonConvert.DeserializeObject<List<Employee>>(json);
                if (items != null)
                {
                    var cacheOptions = new MemoryCacheEntryOptions()
                   .SetSlidingExpiration(TimeSpan.FromSeconds(30))
                   .SetAbsoluteExpiration(TimeSpan.FromSeconds(300))
                   .SetPriority(CacheItemPriority.NeverRemove)
                   .SetSize(2048);
                    _cache.Set(cacheKey, items, cacheOptions);

                }
            }
        }
        private void UpdateCache(List<Employee> employees)
        {
            _cache.Remove(cacheKey);
            var cacheOptions = new MemoryCacheEntryOptions()
                   .SetSlidingExpiration(TimeSpan.FromSeconds(30))
                   .SetAbsoluteExpiration(TimeSpan.FromSeconds(300))
                   .SetPriority(CacheItemPriority.NeverRemove)
                   .SetSize(2048);
            _cache.Set(cacheKey, employees, cacheOptions);
        }
    }
}
