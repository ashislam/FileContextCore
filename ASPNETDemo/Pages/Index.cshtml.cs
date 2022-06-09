using System;
using ASPNETDemo.Data;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.Extensions.Logging;

namespace ASPNETDemo.Pages
{
    public class IndexModel : PageModel
    {
        private readonly ILogger<IndexModel> _logger;
        private readonly Db _db;

        public IndexModel(ILogger<IndexModel> logger, Db db)
        {
            _logger = logger;
            _db = db;
        }

        public void OnGet()
        {
            try
            {
                _db.Users.Add(new User { Email = "asislam@wcgclinical.com", Username = "asislam" });
                _db.SaveChanges();
            }
            catch (Exception ex)
            {
                throw ex;
            }
        }
    }
}
