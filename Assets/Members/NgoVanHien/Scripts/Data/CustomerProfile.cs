using System;
using UnityEngine;

namespace DungeonEater.Tavern
{
    /// <summary>
    /// 1 loại thực khách làng Dun (README: Nông dân, Thương nhân, Mạo hiểm gia, Cô gái làng, Bà lão, Lữ khách bí ẩn).
    /// Mỗi loại có độ kiên nhẫn riêng.
    /// </summary>
    [Serializable]
    public class CustomerProfile
    {
        public string displayName = "Khách";
        public Sprite idleSprite;
        public Sprite eatingSprite;
        /// <summary>Số giây khách chờ món trước khi bỏ về: lấy từ restaurant_config.json (customerProfiles).</summary>
        [NonSerialized] public float patience;
    }
}
