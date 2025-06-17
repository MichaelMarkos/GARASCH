using NewGaras.Infrastructure.Models.Admin;
using NewGarasAPI.Models.Admin;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using static NewGaras.Domain.Helper.TreeStructure;
using TreeViewDto = NewGaras.Domain.Helper.TreeStructure.TreeViewDto;

namespace NewGaras.Domain.Helper
{
    public static class Common
    {
        public static List<TreeViewDto> BuildTreeViews(string pid, IList<TreeViewDto> candicates)
        {
            var subs = candicates.Where(c => c.parentId == pid).ToList();
            if (subs.Count() == 0)
            {
                return new List<TreeViewDto>();
            }
            foreach (var i in subs)
            {
                i.subs = BuildTreeViews(i.id, candicates);
            }
            return subs;
        }

        public static List<TreeViewDto2> BuildTreeViews2(string pid, IList<TreeViewDto2> candicates)
        {
            var subs = candicates.Where(c => c.parentId == pid).ToList();
            if (subs.Count() == 0)
            {
                return new List<TreeViewDto2>();
            }
            foreach (var i in subs)
            {
                i.subs = BuildTreeViews2(i.id, candicates);
            }
            return subs;
        }
    }
}
