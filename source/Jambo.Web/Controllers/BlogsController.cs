﻿using Jambo.Application.Commands;
using Jambo.Application.Commands.Blogs;
using Jambo.Application.Queries;
using Jambo.Domain.Aggregates.Blogs;
using Jambo.Domain.Exceptions;
using MediatR;
using Microsoft.AspNetCore.Mvc;
using System;
using System.Collections.Generic;
using System.Net;
using System.Net.Http;
using System.Threading.Tasks;

namespace Jambo.Web.Controllers
{
    [Route("api/[controller]")]
    public class BlogsController : Controller
    {
        private readonly IMediator _mediator;
        private readonly IBlogQueries _blogQueries;

        public BlogsController(IMediator mediator, IBlogQueries blogQueries)
        {
            _mediator = mediator ?? throw new ArgumentNullException(nameof(mediator));
            _blogQueries = blogQueries ?? throw new ArgumentNullException(nameof(blogQueries));
        }

        [HttpGet]
        public async Task<IActionResult> Get()
        {
            var blogs = await _blogQueries.GetBlogsAsync();

            return Ok(blogs);
        }

        [HttpGet("{id}", Name = "GetBlog")]
        public async Task<IActionResult> Get(Guid id)
        {
            var blog = await _blogQueries.GetBlogAsync(id);

            return Ok(blog);
        }

        [HttpPost]
        public async Task<IActionResult> Post([FromBody]CreateBlogCommand command)
        {
            Guid id = await _mediator.Send(command);

            return CreatedAtRoute("GetBlog", new { id = id }, id);
        }

        [HttpPatch("Enable")]
        public async Task<IActionResult> Enable([FromBody]EnableBlogCommand command)
        {
            if (!ModelState.IsValid)
                return BadRequest(ModelState);

            await _mediator.Send(command);
            return (IActionResult)Ok();
        }

        [HttpPatch("Disable")]
        public async Task<IActionResult> Disable([FromBody]DisableBlogCommand command)
        {
            await _mediator.Send(command);
            return (IActionResult)Ok();
        }

        [HttpPatch("UpdateUrl")]
        public async Task<IActionResult> UpdateUrl([FromBody]UpdateBlogUrlCommand command)
        {
            await _mediator.Send(command);
            return (IActionResult)Ok();
        }
    }
}