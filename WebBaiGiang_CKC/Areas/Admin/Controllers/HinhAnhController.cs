using elFinder.NetCore.Drivers.FileSystem;
using elFinder.NetCore;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http.Extensions;
using Microsoft.AspNetCore.Mvc;
using WebBaiGiang_CKC.Data;

namespace WebBaiGiang_CKC.Areas.Admin.Controllers
{
    [Area("Admin")]
    [Authorize(Roles = "Admin")]

    public class HinhAnhController : Controller
    {


        private readonly WebBaiGiangContext _context;
        protected readonly IWebHostEnvironment _env;
        public HinhAnhController(WebBaiGiangContext context, IWebHostEnvironment env)
        {
            _context = context;
            _env = env;
        }

        // Url để client-side kết nối đến backend
        // /el-finder-file-system/connector
        [Route("/file-manager-connector")]
        public async Task<IActionResult> Connector()
        {
            var connector = GetConnector();
            return await connector.ProcessAsync(Request);
        }

        // Địa chỉ để truy vấn thumbnail
        // /el-finder-file-system/thumb
        [Route("/file-manager-thumb/{hash}")]
        public async Task<IActionResult> Thumbs(string hash)
        {
            var connector = GetConnector();
            return await connector.GetThumbnailAsync(HttpContext.Request, HttpContext.Response, hash);
        }

        private Connector GetConnector()
        {
            // Thư mục gốc lưu trữ là wwwwroot/files (đảm bảo có tạo thư mục này)
            string pathroot = "UpLoads";
            string requestUrl = "contents";
            var driver = new FileSystemDriver();

            string absoluteUrl = UriHelper.BuildAbsolute(Request.Scheme, Request.Host);
            var uri = new Uri(absoluteUrl);

            // .. ... UpLoads
            //_env.ContentRootPath => Lesson_CKC
            string rootDirectory = Path.Combine(_env.ContentRootPath, pathroot);

            // https://localhost:5001/files/
            string url = $"{uri.Scheme}://{uri.Authority}/{requestUrl}/";
            string urlthumb = $"{uri.Scheme}://{uri.Authority}/file-manager-thumb/";


            var root = new RootVolume(rootDirectory, url, urlthumb)
            {
                //IsReadOnly = !User.IsInRole("Administrators")
                IsReadOnly = false, // Can be readonly according to user's membership permission
                IsLocked = false, // If locked, files and directories cannot be deleted, renamed or moved
                Alias = "Thư mục", // Beautiful name given to the root/home folder
                                   //MaxUploadSizeInKb = 2048, // Limit imposed to user uploaded file <= 2048 KB
                                   //LockedFolders = new List<string>(new string[] { "Folder1" }
                ThumbnailSize = 100,
            };


            driver.AddRoot(root);

            return new Connector(driver)
            {
                // This allows support for the "onlyMimes" option on the client.
                MimeDetect = MimeDetectOption.Internal
            };
        }
        // GET: Admin/Images
        [Route("Admin/Images/file-manager")]
        public IActionResult Index()
        {
            return View();
        }



    }
}
