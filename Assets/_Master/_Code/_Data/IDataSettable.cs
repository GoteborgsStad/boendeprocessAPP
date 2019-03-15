using UnityEngine;
using System.Collections;
using System.Collections.Generic;

namespace ius
{
	public interface IDataSettable 
	{
		void SetData(Dictionary<string, object> data);
	}
}