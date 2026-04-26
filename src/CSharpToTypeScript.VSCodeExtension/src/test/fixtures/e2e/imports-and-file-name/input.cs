using System.Collections.Generic;

namespace Contracts
{
    public class IOrderDto
    {
        public IExternalModel Customer { get; set; }
        public IList<IExternalModel> Related { get; set; }
    }
}
