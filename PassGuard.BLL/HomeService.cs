using System.Collections.Generic;
using PassGuard.DAL;
using PassGuard.Models;

namespace PassGuard.BLL
{
    public class HomeService
    {
        private readonly HomeRepository _repo;

        public HomeService(HomeRepository repo)
        {
            _repo = repo;
        }

        public List<Home> GetAllWithDetails()
        {
            return _repo.GetAllWithDetails();
        }

        public Home? GetFullDetails(int id)
        {
            return _repo.GetFullDetails(id);
        }

        public Home? GetById(int id)
        {
            return _repo.GetById(id);
        }

        public Home? GetByAddressAndEstateId(string address, int estateId)
        {
            return _repo.GetByAddressAndEstateId(address, estateId);
        }

        public Home? GetByOwnerUserId(string ownerUserId)
        {
            return _repo.GetByOwnerUserId(ownerUserId);
        }

        public void Add(Home home)
        {
            _repo.Add(home);
        }

        public void Update(Home home)
        {
            _repo.Update(home);
        }

        public void Delete(int id)
        {
            _repo.Delete(id);
        }
    }
}
