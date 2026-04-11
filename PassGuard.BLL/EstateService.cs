using PassGuard.DAL;
using PassGuard.Models;

namespace PassGuard.BLL
{
    public class EstateService
    {
        private readonly EstateRepository _repo;

        public EstateService(EstateRepository repo)
        {
            _repo = repo;
        }

        public Estate? GetByName(string name)
        {
            return _repo.GetByName(name);
        }

        public Estate? GetById(int id)
        {
            return _repo.GetById(id);
        }

        public void Add(Estate estate)
        {
            _repo.Add(estate);
        }

        public void Update(Estate estate)
        {
            _repo.Update(estate);
        }
    }
}