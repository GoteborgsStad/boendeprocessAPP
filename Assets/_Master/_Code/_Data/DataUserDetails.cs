using UnityEngine;
using System.Collections;
using System.Collections.Generic;

namespace ius
{
	public enum UserSex
	{
		NotKnown = 0,
		Male = 1,
		Female = 2,
		NotApplicable = 9
	}

	public class DataUserDetails : DataBaseObject 
	{
		public string FirstName { get; private set; }
		public string LastName { get; private set; }
		public string FullName { get; private set; }
		public string DisplayName { get; private set; }

		public string ImageURL { get; private set; }
		public Sprite Image { get; private set; }

		public string Email { get; private set; }
		public string StreetAddress { get; private set; }
		public string ZipCode { get; private set; }
		public string City { get; private set; }
		public string HomePhone { get; private set; }
		public string CellPhone { get; private set; }

		public System.Action OnGetImage;
		private float mNextFetchImage;

		private const string KEY_FIRST_NAME = "first_name";
		private const string KEY_LAST_NAME = "last_name";
		private const string KEY_FULL_NAME = "full_name";
		private const string KEY_DISPLAY_NAME = "display_name";
		private const string KEY_SEX = "sex";
		private const string KEY_IMAGE_URL = "image_url";

		private const string KEY_EMAIL = "email";
		private const string KEY_STREET_ADDRESS = "street_address";
		private const string KEY_ZIP_CODE = "zip_code";
		private const string KEY_CITY = "city";
		private const string KEY_HOME_PHONE = "home_phone_number";
		private const string KEY_CELL_PHONE = "cell_phone_number";

		private static Dictionary<int, DataUserDetails> mUserDetails = new Dictionary<int, DataUserDetails>();

		private DataUserDetails() { } // Only DataUserDetails.GetDetails should call this

		public override void SetData(Dictionary<string, object> data)
		{
			base.SetData(data);

			FirstName = Get<string>(data, KEY_FIRST_NAME);
			LastName = Get<string>(data, KEY_LAST_NAME);
			FullName = Get<string>(data, KEY_FULL_NAME);
			DisplayName = Get<string>(data, KEY_DISPLAY_NAME);

			string imageURL = Get<string>(data, KEY_IMAGE_URL);

			if (imageURL != ImageURL)
			{
				ImageURL = imageURL;
				FetchImage();
			}

			Email = Get<string>(data, KEY_EMAIL);
			StreetAddress = Get<string>(data, KEY_STREET_ADDRESS);
			ZipCode = Get<string>(data, KEY_ZIP_CODE);
			City = Get<string>(data, KEY_CITY);
			HomePhone = Get<string>(data, KEY_HOME_PHONE);
			CellPhone = Get<string>(data, KEY_CELL_PHONE);
		}

		public void FetchImage()
		{
			if (string.IsNullOrEmpty(ImageURL))
			{
				Image = null;
			}
			else if (Time.time > mNextFetchImage)
			{
				mNextFetchImage = Time.time + 5f;
				Backend.CallURL(ImageURL, OnReceiveImage);
			}
		}

		private void OnReceiveImage(WebCall webCall)
		{
			Texture2D texture = PhotoConverter.ConvertToSquare(webCall.ResponseImage, 128);
			texture.name = "User" + ID + "Picture";

			Image = Sprite.Create(texture, new Rect(0, 0, texture.width, texture.height), new Vector2(0.5f, 0.5f));
			Image.name = texture.name;

			if (OnGetImage != null)
				OnGetImage();
		}

		public static DataUserDetails GetDetails(Dictionary<string, object> data)
		{
			int id = GetInt(data, KEY_ID);

			if (mUserDetails.ContainsKey(id))
			{
				DataUserDetails details = mUserDetails[id];
				details.SetData(data);
				return details;
			}

			DataUserDetails newDetails = new DataUserDetails();
			newDetails.SetData(data);
			mUserDetails[id] = newDetails;
			return newDetails;
		}
	}
}