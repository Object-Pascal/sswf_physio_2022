using Core.Domain;
using System.Collections.Generic;

namespace Core.DomainServices
{
    public interface IRemarkRepository
    {
        IEnumerable<Remark> GetAll(string patientNumber);
        bool AddRemark(Remark remark);
    }
}