﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using SpellFire.Primer.Solutions;

namespace SpellFire.Primer.Solutions
{
	public abstract class MultiboxSolution : Solution
	{
		public IEnumerable<Client> clients;

		protected MultiboxSolution(IEnumerable<Client> clients) : base(clients.First())
		{
			this.clients = clients;
		}

		public IEnumerable<Client> Slaves => clients.Where(c => c != me);
	}
}
