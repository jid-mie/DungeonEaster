using System;
using System.Collections.Generic;
using UnityEngine;

namespace DungeonEater.TranHuuHau.Data
{
    [Serializable]
    public class DialogueEntry
    {
        public string speakerName;
        public string speakerRole;
        [TextArea(2, 5)] public string[] lines;
    }

    /// <summary>
    /// Kịch bản hội thoại đầy đủ cho 10 NPC làng Londo theo báo cáo GDD.
    /// Gồm: Trưởng làng Godwin, Gia đình 1 (Sam, Nana, Leaf),
    /// Gia đình 2 (Gam, Ana, Vincent), Gia đình 3 (Dam, Sana, Vini).
    /// </summary>
    public static class VillageDialogueData
    {
        public static DialogueEntry GetElderGodwin()
        {
            return new DialogueEntry
            {
                speakerName = "Trưởng làng Godwin",
                speakerRole = "Lão trưởng làng Londo",
                lines = new string[]
                {
                    "Chào mừng cậu dũng sĩ đã trở về bình an từ lòng hầm ngục!",
                    "Ngôi làng Londo nghèo khó này mang ơn cứu mạng của cậu rất nhiều.",
                    "Nhờ có các món ăn ngon lành bổ dưỡng từ nguyên liệu quái vật của cậu, dân làng ta mới dần hồi phục sức sống.",
                    "Ta đã để lại căn nhà hoang cho cậu làm quán ăn, hy vọng cậu sẽ tiếp tục gắn bó cùng ngôi làng.",
                    "Cần mua bình máu, lựu đạn nổ hay dược thảo thì cứ bước vào tiệm thuốc của ta ngay đằng sau nhé!"
                }
            };
        }

        public static DialogueEntry GetSam()
        {
            return new DialogueEntry
            {
                speakerName = "Sam",
                speakerRole = "Chồng - Tiều phu & Nông dân (Gia đình 1)",
                lines = new string[]
                {
                    "Chào cậu đầu bếp dũng cảm! Tôi là Sam, sống cùng vợ Nana và bé Leaf bên căn nhà gỗ này.",
                    "Từ ngày chướng khí độc hại từ ngục tối tràn lên, hoa màu trên cánh đồng đều chết rũ hết cả.",
                    "Trước đây cả nhà tôi phải nhai vỏ cây qua ngày... May nhờ có quán ăn của cậu mở ra cứu rỗi gia đình tôi.",
                    "Thịt nướng xông khói cậu làm đậm vị thật đấy, ăn một miếng là tôi vác rìu đốn củi cả ngày không biết mệt!"
                }
            };
        }

        public static DialogueEntry GetNana()
        {
            return new DialogueEntry
            {
                speakerName = "Nana",
                speakerRole = "Vợ - Người nội trợ đảm đang (Gia đình 1)",
                lines = new string[]
                {
                    "Ôi, cậu đầu bếp tốt bụng đấy à! Tôi là Nana, vợ của anh Sam.",
                    "Con bé Leaf nhà tôi bình thường biếng ăn lắm, thế mà hôm nọ nó ăn sạch veo một bát lớn súp nấm dạ quang cậu nấu!",
                    "Cậu xuống hầm ngục nhớ mang theo đủ bình máu và đồ bảo hộ nhé, cả làng đều lo lắng cho sự an toàn của cậu đấy."
                }
            };
        }

        public static DialogueEntry GetLeaf()
        {
            return new DialogueEntry
            {
                speakerName = "Bé Leaf",
                speakerRole = "Con gái nhỏ tinh nghịch (Gia đình 1)",
                lines = new string[]
                {
                    "A! Chú đầu bếp siêu ngầu đã về rồi!",
                    "Cháu là Leaf đây ạ! Chú ơi, dưới ngục tối sâu hoắm có quái vật hình cây nấm biết nhảy múa thật không chú?",
                    "Món thạch Slime dẻo chú làm ngọt lịm như kẹo ấy, cháu thích mê luôn! Khi nào quán mở cửa lại nhớ phần cháu một đĩa to nhé!"
                }
            };
        }

        public static DialogueEntry GetGam()
        {
            return new DialogueEntry
            {
                speakerName = "Gam",
                speakerRole = "Chồng - Cựu thợ rèn kiêm dân quân (Gia đình 2)",
                lines = new string[]
                {
                    "Ha ha ha! Cậu em đầu bếp quả cảm! Ta là Gam, chủ căn nhà số 2.",
                    "Năm ngoái ta từng vác búa lớn tính liều mình xuống hầm ngục kiếm chút thịt rừng, nhưng lũ quái xương cung thủ bắn dữ dội quá, đành ôm vết thương bỏ chạy.",
                    "Thấy cậu một mình vung kiếm hạ gục lũ quái hung hãn, ta thực sự nể phục!",
                    "Lúc nào rảnh rỗi ghé qua ta, ta sẽ chỉ cho vài mẹo rèn giũa lưỡi kiếm bén ngót để chém quái ngọt hơn!"
                }
            };
        }

        public static DialogueEntry GetAna()
        {
            return new DialogueEntry
            {
                speakerName = "Ana",
                speakerRole = "Vợ - Người dệt vải làng Londo (Gia đình 2)",
                lines = new string[]
                {
                    "Chào cậu dũng sĩ! Tôi là Ana, vợ nhà anh Gam.",
                    "Trưởng làng Godwin quả là có con mắt nhìn người khi đổi căn nhà hoang cho cậu định cư.",
                    "Từ ngày có quán ăn, làng Londo không khí rộn ràng hẳn lên, không còn ảm đạm u uất như trước nữa.",
                    "Tôi đang dệt thêm vài tấm khăn trải bàn mới thật đẹp để đem tặng quán ăn của cậu đây!"
                }
            };
        }

        public static DialogueEntry GetVincent()
        {
            return new DialogueEntry
            {
                speakerName = "Vincent",
                speakerRole = "Con trai - Thiếu niên tập sự (Gia đình 2)",
                lines = new string[]
                {
                    "Chào anh đầu bếp! Em là Vincent, con trai của bố Gam!",
                    "Em đang ngày đêm luyện tập kỹ thuật chém kiếm hình cung và lăn xả né đòn như anh đấy!",
                    "Anh kể cho em nghe về Boss Rắn khổng lồ ở tầng 1 đi! Nó có phun nọc độc không anh?",
                    "Lớn lên em nhất định sẽ trở thành một dũng sĩ thiện chiến như anh, cùng anh xuống tầng 5 đánh bại thần Gwyn để giải thoát cho ngôi làng!"
                }
            };
        }

        public static DialogueEntry GetDam()
        {
            return new DialogueEntry
            {
                speakerName = "Dam",
                speakerRole = "Chồng - Người gác cổng làng (Gia đình 3)",
                lines = new string[]
                {
                    "Chào cậu bạn! Ta là Dam, phụ trách canh giữ lối mòn duy nhất dẫn qua dãy núi ra thế giới bên ngoài.",
                    "Ngôi làng Londo địa thế hiểm trở bốn bề vách đá, thương nhân phương xa xưa nay hiếm ai muốn đặt chân tới.",
                    "Nhưng gần đây hương vị món ăn kỳ lạ thơm nức mũi của cậu đã bay xa rồi đấy!",
                    "Mấy hôm trước ta thấy có vài thương buôn giàu có ghé hỏi thăm đường vào làng. Tiệm ăn của cậu sắp đón khách quý rồi!"
                }
            };
        }

        public static DialogueEntry GetSana()
        {
            return new DialogueEntry
            {
                speakerName = "Sana",
                speakerRole = "Vợ - Thầy thuốc thảo mộc (Gia đình 3)",
                lines = new string[]
                {
                    "Chào cậu thợ săn - đầu bếp! Tôi là Sana, vợ của Dam.",
                    "Tôi chuyên hái lượm và nghiên cứu các loại rêu lọc độc cùng cỏ dược quanh bìa rừng.",
                    "Ám khí dungeon rất độc hại, nhưng nhờ cậu tiêu diệt bớt quái vật nên chúng tôi mới an tâm hái thuốc.",
                    "Nếu nhặt được củ rêu lọc độc ở tầng đầm lầy, hãy nhớ giữ lại nhé, nó có thể trung hòa vị đắng và khử độc món ăn rất tốt đấy!"
                }
            };
        }

        public static DialogueEntry GetVini()
        {
            return new DialogueEntry
            {
                speakerName = "Vini",
                speakerRole = "Con gái nhỏ nhút nhát (Gia đình 3)",
                lines = new string[]
                {
                    "...Dạ... em chào anh đầu bếp ạ... Em là Vini...",
                    "Mỗi lần nhìn lên ngọn núi sương mù nơi miệng hang hầm ngục, em đều thấy lạnh sống lưng và sợ hãi...",
                    "Nhưng thấy anh trở về an toàn với nụ cười trên môi, em thấy lòng nhẹ nhõm hơn nhiều.",
                    "Anh vào ngục tối nhớ cẩn thận bẫy chông sàn nhà nhé... Em chúc anh luôn bình an!"
                }
            };
        }
    }
}
