﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using fIT.WebApi.Entities;
using fIT.WebApi.Repository.Interfaces.CRUD;
using fIT.WebApi.Repository.Interfaces.CRUD.MultiId;
using fIT.WebApi.Repository.Interfaces.CRUD.SingleID;

namespace fIT.WebApi.Repository.Interfaces
{
    internal interface IPracticeRepository : IRepositoryAddAndDelete<Practice, int>, IRepositoryFindAll<Practice>, IRepositoryFindSingle<Practice, int>, IRepositoryUpdate<Practice, int>
    {

  }
}
