﻿using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Web;
using fIT.WebApi.Repository.Interfaces.CRUD;

namespace fIT.WebApi.Entities
{
  public class RefreshToken: IEntity<string>
  {
    [Key]
    public string Id { get; set; }
    [Required]
    [MaxLength(50)]
    public string Subject { get; set; }
    [Required]
    [MaxLength(50)]
    public string ClientId { get; set; }
    public DateTime IssuedUtc { get; set; }
    public DateTime ExpiresUtc { get; set; }
    [Required]
    public string ProtectedTicket { get; set; }
  }
}