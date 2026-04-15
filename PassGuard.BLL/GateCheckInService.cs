using System.Collections.Generic;
using PassGuard.DAL;
using PassGuard.Models;

namespace PassGuard.BLL
{
    public class GateCheckInService
    {
        private readonly GateCheckInRepository _repo;

        public GateCheckInService(GateCheckInRepository repo)
        {
            _repo = repo;
        }

        public GateCheckIn? GetByVisitPassId(int visitPassId)
        {
            return _repo.GetByVisitPassId(visitPassId);
        }

        public GateCheckIn? GetById(int id)
        {
            return _repo.GetById(id);
        }

        public void Add(GateCheckIn gateCheckIn)
        {
            _repo.Add(gateCheckIn);
        }

        public void Update(GateCheckIn gateCheckIn)
        {
            _repo.Update(gateCheckIn);
        }

        public void Delete(int id)
        {
            _repo.Delete(id);
        }
    }
}
