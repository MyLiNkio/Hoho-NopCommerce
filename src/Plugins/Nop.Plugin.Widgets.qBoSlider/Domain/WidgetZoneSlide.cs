﻿//Copyright 2020 Alexey Prokhorov

//Licensed under the Apache License, Version 2.0 (the "License");
//you may not use this file except in compliance with the License.
//You may obtain a copy of the License at

//    http://www.apache.org/licenses/LICENSE-2.0

//Unless required by applicable law or agreed to in writing, software
//distributed under the License is distributed on an "AS IS" BASIS,
//WITHOUT WARRANTIES OR CONDITIONS OF ANY KIND, either express or implied.
//See the License for the specific language governing permissions and
//limitations under the License.

using Nop.Core;
using Nop.Core.Domain.Localization;

namespace Nop.Plugin.Widgets.qBoSlider.Domain
{
    /// <summary>
    /// Represents widget zone slide relation
    /// </summary>
    public class WidgetZoneSlide : BaseEntity, ILocalizedEntity
    {
        /// <summary>
        /// Gets or sets the widget zone id number
        /// </summary>
        public int WidgetZoneId { get; set; }

        /// <summary>
        /// Gets or sets UseOnMobile indicator
        /// </summary>
        public bool UseOnMobile { get; set; }

        /// <summary>
        /// Gets or sets the slide id number
        /// </summary>
        public int SlideId { get; set; }

        /// <summary>
        /// Gets or sets the id of Universal slide to which that UseOnMobile slide belong.
        /// This paramenet evaluating in pair with "UseOnMobile". If UseOnMobile=true - than the record is Child slide (dependable from Main) and refer to Main slider id, otherwise it is Main slide in it can refer point on child. Element has no child if value null or 0.
        /// </summary>
        public int MainOrChildSlideId { get; set; }

        /// <summary>
        /// Gets or sets slide displaying order for current widget zone
        /// </summary>
        public int DisplayOrder { get; set; }
    }
}
