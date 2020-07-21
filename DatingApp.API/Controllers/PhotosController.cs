using System.Linq;
using System.Security.Claims;
using System.Threading.Tasks;
using AutoMapper;
using CloudinaryDotNet;
using CloudinaryDotNet.Actions;
using DatingApp.API.Controllers.Models;
using DatingApp.API.Data;
using DatingApp.API.Dtos;
using DatingApp.API.Helpers;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Options;

namespace DatingApp.API.Controllers
{
  [Authorize]
  [Route("api/users/{userid}/photos")]
  [ApiController]
  public class PhotosController : ControllerBase
  {
    private readonly IDatingRepository _repo;
    private readonly IMapper _mapper;
    private readonly IOptions<CloudinarySettings> _cloudinarySettings;
    private readonly Cloudinary _cloudinary;

    public PhotosController(IDatingRepository repo, IMapper mapper, IOptions<CloudinarySettings> cloudinarySettings)
    {
      _cloudinarySettings = cloudinarySettings;
      _mapper = mapper;
      _repo = repo;

      Account acc = new Account(
          _cloudinarySettings.Value.CloudName,
          _cloudinarySettings.Value.ApiKey,
          _cloudinarySettings.Value.ApiSecret
      );

      _cloudinary = new Cloudinary(acc);
    }

    [HttpGet("{id}", Name = "GetPhoto")]
    public async Task<IActionResult> GetPhoto(int id)
    {
      var photoFromRepo = await _repo.GetPhoto(id);
      var photo = _mapper.Map<PhotoForReturnDto>(photoFromRepo);
      return Ok(photo);
    }

    [HttpPost]
    public async Task<IActionResult> AddPhotoForUser(int userId,
    [FromForm] PhotoForCreationDto photoForCreationDto)
    {
      if (userId != int.Parse(User.FindFirst(ClaimTypes.NameIdentifier).Value))
        return Unauthorized();

      var userFromRepo = await _repo.GetUser(userId);
      var file = photoForCreationDto.File;

      var result = new ImageUploadResult();

      if (file.Length > 0)
      {
        using (var stream = file.OpenReadStream())
        {
          var uploadParams = new ImageUploadParams
          {
            File = new FileDescription(file.Name, stream),
            Transformation = new Transformation().Width(128).Height(128).Crop("fill").Gravity("face")
          };

          result = _cloudinary.Upload(uploadParams);
        }
      }

      photoForCreationDto.Url = result.Url.ToString();
      photoForCreationDto.PublicId = result.PublicId;

      var photo = _mapper.Map<Photo>(photoForCreationDto);


      if (!userFromRepo.Photos.Any(u => u.IsMain))
        photo.IsMain = true;
      userFromRepo.Photos.Add(photo);
      if (await _repo.SaveAll())
      {
        var photoToReturn = _mapper.Map<PhotoForReturnDto>(photo);
        return CreatedAtRoute("GetPhoto", new { userId = userId, id = photo.Id }, photoToReturn);
      }
      return BadRequest("Could not upload the photo");
    }
    [HttpPost("{id}/setMain")]
    public async Task<IActionResult> SetMainPhoto(int userId, int id)
    {
      if (userId != int.Parse(User.FindFirst(ClaimTypes.NameIdentifier).Value))
        return Unauthorized();

      var user = await _repo.GetUser(userId);

      if (!user.Photos.Any(p => p.Id == id))
        return Unauthorized();

      var photoFromRepo = await _repo.GetPhoto(id);
      if (photoFromRepo.IsMain)
        return BadRequest("This is already the main photo!");

      var mainPhoto = await _repo.GetMainPhotoForUser(userId);
      mainPhoto.IsMain = false;
      photoFromRepo.IsMain = true;

      if (await _repo.SaveAll())
        return NoContent();

      return BadRequest("Could not set photo to main");
    }
    [HttpDelete("{id}")]
    public async Task<IActionResult> DeletePhoto(int userId, int id)
    {
      if (userId != int.Parse(User.FindFirst(ClaimTypes.NameIdentifier).Value))
        return Unauthorized();

      var user = await _repo.GetUser(userId);

      if (!user.Photos.Any(p => p.Id == id))
        return Unauthorized();

      var photoFromRepo = await _repo.GetPhoto(id);
      if (photoFromRepo.IsMain)
        return BadRequest("You cannot delete your main photo!");

      if (photoFromRepo.PublicId != null)
      {
        var deleteParams = new DeletionParams(photoFromRepo.PublicId);
        var result = this._cloudinary.Destroy(deleteParams);

        if (result.Result == "ok")
        {
          _repo.Delete(photoFromRepo);
        }
      }
      else
      {
        _repo.Delete(photoFromRepo);
      }


      if (await _repo.SaveAll())
        return Ok();

      return BadRequest("Unable to delete photo!");
    }
  }
}