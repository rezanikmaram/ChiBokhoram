using System.Threading;
using System.Threading.Tasks;
using Services.PublicService.Abstract;

namespace WebPanel.Controllers
{
    public class TestStuffController
    {
        public IAllocationEquPublicService allocationEquPublicService { get; set; }

        public TestStuffController(IAllocationEquPublicService allocationEquPublicService)
        {
            this.allocationEquPublicService = allocationEquPublicService;
        }

        public async Task Run(CancellationToken token)
        {
            await allocationEquPublicService.UpdateFactorNumber(token);
        }
    }
}
