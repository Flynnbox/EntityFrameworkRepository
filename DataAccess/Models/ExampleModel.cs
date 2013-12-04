using System;
using System.ComponentModel.DataAnnotations;

namespace DataAccess.Models
{
	[Table("exampleTable")]
	public class ExampleModel
	{
		[Key]
		public Guid ModelGuid { get; set; }

		[Required]
		public string Name { get; set; }
	}
}