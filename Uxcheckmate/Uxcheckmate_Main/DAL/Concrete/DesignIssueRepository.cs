using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;

using Uxcheckmate_Main.DAL.Abstract;
using Uxcheckmate_Main.Models;

namespace Uxcheckmate_Main.DAL.Concrete
{
    public class DesignIssueRepository : Repository<DesignIssue>, IDesignIssueRepository
    {
        public DesignIssueRepository(UxCheckmateDbContext ctx) : base(ctx)
        {

        }
    }
}