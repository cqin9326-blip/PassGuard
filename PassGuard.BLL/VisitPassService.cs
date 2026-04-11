using System.Collections.Generic;
using PassGuard.DAL;
using PassGuard.Models;

namespace PassGuard.BLL
{
    public class VisitPassService
    {
        private readonly VisitPassRepository _repo;

        public VisitPassService(VisitPassRepository repo)
        {
            _repo = repo;
        }

        public List<VisitPass> GetAllWithDetails()
        {
            return _repo.GetAllWithDetails();
        }

        public VisitPass? GetFullDetails(int id)
        {
            return _repo.GetFullDetails(id);
        }

        public VisitPass? GetById(int id)
        {
            return _repo.GetById(id);
        }

        public VisitPass? GetFirstByHomeId(int homeId)
        {
            return _repo.GetFirstByHomeId(homeId);
        }

        public void Add(VisitPass visitPass)
        {
            _repo.Add(visitPass);
        }

        public void Update(VisitPass visitPass)
        {
            _repo.Update(visitPass);
        }

        public void Delete(int id)
        {
            _repo.Delete(id);
        }
    }
}