/**
 * Official browser brand marks. Labels match ParseBrowser in
 * GetLinkAnalyticsHandler.cs.
 *
 * Every gradient id is prefixed per browser (`ff-A`, `edge-a`, …). The upstream
 * artwork all uses bare ids like `a`/`b`, and SVG ids are document-global — two
 * unprefixed icons on the same page would resolve each other's gradients and
 * paint the wrong colours.
 */

interface IconProps {
  className?: string;
}

function Chrome({ className }: IconProps) {
  return (
    <svg viewBox="0 0 24 24" className={className} role="img" aria-label="Chrome">
      {/* Three 120° sectors: red on top, green lower-left, yellow lower-right */}
      <path fill="#EA4335" d="M12 12 L2.474 6.5 A11 11 0 0 1 21.526 6.5 Z" />
      <path fill="#34A853" d="M12 12 L12 23 A11 11 0 0 1 2.474 6.5 Z" />
      <path fill="#FBBC04" d="M12 12 L21.526 6.5 A11 11 0 0 1 12 23 Z" />
      <circle cx="12" cy="12" r="4.9" fill="#FFF" />
      <circle cx="12" cy="12" r="3.8" fill="#4285F4" />
    </svg>
  );
}

function Firefox({ className }: IconProps) {
  return (
    <svg viewBox="-34.5 0 1022 1022" className={className} role="img" aria-label="Firefox">
      <defs>
        <radialGradient id="ff-A" cx="-14516" cy="-8331.1" r="450.88" fx="-14544" gradientTransform="matrix(.76 .03 -.05 1.12 11552 10071)" gradientUnits="userSpaceOnUse">
          <stop offset=".1" stopColor="#ffea00" /><stop offset=".17" stopColor="#ffde00" /><stop offset=".28" stopColor="#ffbf00" /><stop offset=".43" stopColor="#ff8e00" /><stop offset=".77" stopColor="#ff272d" /><stop offset=".87" stopColor="#e0255a" /><stop offset=".95" stopColor="#cc2477" /><stop offset="1" stopColor="#c42482" />
        </radialGradient>
        <radialGradient id="ff-B" cx="-7564.7" cy="-7923.6" r="791.23" gradientTransform="matrix(1.23 0 0 1.23 9929.8 9899.4)" gradientUnits="userSpaceOnUse">
          <stop offset="0" stopColor="#00ccda" /><stop offset=".22" stopColor="#0083ff" /><stop offset=".26" stopColor="#007af9" /><stop offset=".33" stopColor="#0060e8" /><stop offset=".33" stopColor="#005fe7" /><stop offset=".44" stopColor="#2639ad" /><stop offset=".52" stopColor="#401e84" /><stop offset=".57" stopColor="#4a1475" />
        </radialGradient>
        <linearGradient id="ff-C" x1="540.67" x2="349.23" y1="729.19" y2="102.96" gradientUnits="userSpaceOnUse">
          <stop offset="0" stopColor="#000f43" stopOpacity=".4" /><stop offset=".48" stopColor="#001962" stopOpacity=".17" /><stop offset="1" stopColor="#002079" stopOpacity="0" />
        </linearGradient>
        <radialGradient id="ff-D" cx="-8295.9" cy="-6518.6" r="266.89" gradientTransform="matrix(1.22 .12 -.12 1.22 10304 9602)" gradientUnits="userSpaceOnUse">
          <stop offset="0" stopColor="#ffea00" /><stop offset=".5" stopColor="#ff272d" /><stop offset="1" stopColor="#c42482" />
        </radialGradient>
        <radialGradient id="ff-E" cx="-8320.1" cy="-6774.5" r="445.68" gradientTransform="matrix(1.22 .12 -.12 1.22 10304 9602)" gradientUnits="userSpaceOnUse">
          <stop offset="0" stopColor="#ffe900" /><stop offset=".16" stopColor="#ffaf0e" /><stop offset=".32" stopColor="#ff7a1b" /><stop offset=".47" stopColor="#ff4e26" /><stop offset=".62" stopColor="#ff2c2e" /><stop offset=".76" stopColor="#ff1434" /><stop offset=".89" stopColor="#ff0538" /><stop offset="1" stopColor="#ff0039" />
        </radialGradient>
        <radialGradient id="ff-F" cx="-8257" cy="-6361.3" r="408.96" gradientTransform="matrix(1.22 .12 -.12 1.22 10304 9602)" gradientUnits="userSpaceOnUse">
          <stop offset="0" stopColor="#ff272d" /><stop offset=".5" stopColor="#c42482" /><stop offset=".99" stopColor="#620700" />
        </radialGradient>
        <radialGradient id="ff-G" cx="715.19" cy="394.04" r="782.18" fx="743.17" fy="380.21" gradientUnits="userSpaceOnUse">
          <stop offset=".16" stopColor="#ffea00" /><stop offset=".23" stopColor="#ffde00" /><stop offset=".37" stopColor="#ffbf00" /><stop offset=".54" stopColor="#ff8e00" /><stop offset=".76" stopColor="#ff272d" /><stop offset=".8" stopColor="#f92433" /><stop offset=".84" stopColor="#e91c45" /><stop offset=".89" stopColor="#cf0e62" /><stop offset=".94" stopColor="#b5007f" />
        </radialGradient>
        <radialGradient id="ff-H" cx="670.34" cy="31.29" r="891.45" gradientUnits="userSpaceOnUse">
          <stop offset=".28" stopColor="#ffea00" /><stop offset=".4" stopColor="#fd0" /><stop offset=".63" stopColor="#ffba00" /><stop offset=".86" stopColor="#ff9100" /><stop offset=".93" stopColor="#ff6711" /><stop offset=".99" stopColor="#ff4a1d" />
        </radialGradient>
        <linearGradient id="ff-I" x1="150.45" x2="534.39" y1="375.21" y2="316.53" gradientUnits="userSpaceOnUse">
          <stop offset="0" stopColor="#c42482" stopOpacity=".5" /><stop offset=".47" stopColor="#ff272d" stopOpacity=".5" /><stop offset=".49" stopColor="#ff2c2c" stopOpacity=".5" /><stop offset=".68" stopColor="#ff7a1a" stopOpacity=".72" /><stop offset=".83" stopColor="#ffb20d" stopOpacity=".87" /><stop offset=".94" stopColor="#ffd605" stopOpacity=".96" /><stop offset="1" stopColor="#ffe302" />
        </linearGradient>
        <linearGradient id="ff-J" x1="142.58" x2="102.54" y1="265.27" y2="121.32" gradientUnits="userSpaceOnUse">
          <stop offset="0" stopColor="#891551" stopOpacity=".6" /><stop offset="1" stopColor="#c42482" stopOpacity="0" />
        </linearGradient>
        <linearGradient id="ff-K" x1="220.55" x2="303.03" y1="465.39" y2="580.61" gradientUnits="userSpaceOnUse">
          <stop offset=".01" stopColor="#891551" stopOpacity=".5" /><stop offset=".48" stopColor="#ff272d" stopOpacity=".5" /><stop offset="1" stopColor="#ff272d" stopOpacity="0" />
        </linearGradient>
        <linearGradient id="ff-L" x1="378.25" x2="385.79" y1="667.12" y2="593.1" gradientUnits="userSpaceOnUse">
          <stop offset="0" stopColor="#c42482" /><stop offset=".08" stopColor="#c42482" stopOpacity=".8" /><stop offset=".21" stopColor="#c42482" stopOpacity=".57" /><stop offset=".33" stopColor="#c42482" stopOpacity=".36" /><stop offset=".45" stopColor="#c42482" stopOpacity=".2" /><stop offset=".56" stopColor="#c42482" stopOpacity=".1" /><stop offset=".67" stopColor="#c42482" stopOpacity=".02" /><stop offset=".77" stopColor="#c42482" stopOpacity="0" />
        </linearGradient>
        <linearGradient id="ff-M" x1="620.53" x2="926.19" y1="36.12" y2="719.61" gradientUnits="userSpaceOnUse">
          <stop offset="0" stopColor="#fff14f" /><stop offset=".27" stopColor="#ffee4c" /><stop offset=".45" stopColor="#ffe643" /><stop offset=".61" stopColor="#ffd834" /><stop offset=".76" stopColor="#ffc41e" /><stop offset=".89" stopColor="#ffab02" /><stop offset=".9" stopColor="#ffa900" /><stop offset=".95" stopColor="#ffa000" /><stop offset="1" stopColor="#ff9100" />
        </linearGradient>
        <linearGradient id="ff-N" x1="680.88" x2="536.1" y1="429.21" y2="817.96" gradientUnits="userSpaceOnUse">
          <stop offset="0" stopColor="#ff8e00" /><stop offset=".04" stopColor="#ff8e00" stopOpacity=".86" /><stop offset=".08" stopColor="#ff8e00" stopOpacity=".73" /><stop offset=".13" stopColor="#ff8e00" stopOpacity=".63" /><stop offset=".18" stopColor="#ff8e00" stopOpacity=".56" /><stop offset=".23" stopColor="#ff8e00" stopOpacity=".5" /><stop offset=".28" stopColor="#ff8e00" stopOpacity=".5" /><stop offset=".39" stopColor="#ff8e00" stopOpacity=".48" /><stop offset=".52" stopColor="#ff8e00" stopOpacity=".42" /><stop offset=".68" stopColor="#ff8e00" stopOpacity=".3" /><stop offset=".84" stopColor="#ff8e00" stopOpacity=".17" /><stop offset="1" stopColor="#ff8e00" stopOpacity="0" />
        </linearGradient>
      </defs>
      <path fill="url(#ff-A)" d="M770.28 91.56c-23.95 27.88-35.1 90.64-10.82 154.26s61.5 49.8 84.7 114.67c30.62 85.6 16.37 200.6 16.37 200.6s36.8 106.6 62.47-6.63c56.8-212.7-152.72-410.5-152.72-462.9z" />
      <path fill="url(#ff-B)" d="M478.07 974.64c245.24 0 443.9-199.74 443.9-446s-198.66-446-443.66-446S34.65 282.32 34.65 528.6c-.47 246.53 198.42 446.03 443.42 446.03z" />
      <path fill="url(#ff-C)" d="M810.67 803.64a246.89 246.89 0 01-30.12 18.18 704 704 0 0038.3-63c9.46-10.47 18.13-20.65 25.2-31.65 3.44-5.4 7.3-12.08 11.42-19.82 24.92-44.9 52.4-117.56 53.18-192.2v-5.66a257.6 257.6 0 00-5.71-55.75l.56 4.3-.64-3.3c.37 2 .66 4 1 6 5.1 43.22 1.47 85.37-16.68 116.45l-.87 1.32c9.4-47.23 12.56-99.4 2.1-151.6 0 0-4.2-25.38-35.38-102.44-18-44.35-49.83-80.72-78-107.2-24.7-30.55-47.1-51-59.47-64.06-25.82-27.2-36.64-47.57-41.1-60.87-3.85-1.93-53.14-49.8-57.05-51.63-21.5 33.35-89.16 137.67-57 235.15 14.58 44.17 51.47 90 90.07 115.74 1.7 1.94 23 25 33.1 77.16 10.45 53.85 5 95.86-16.54 158-25.3 54.5-90.07 108.4-150.72 113.9-129.67 11.78-177.15-65.1-177.15-65.1 46.34 18.52 97.57 14.65 128.72-4.56 31.4-19.43 50.4-33.83 65.8-28.15C548.86 648.43 561 632 550.1 615a78.5 78.5 0 00-79.4-34.57c-31.43 5.1-60.23 30-101.4 5.9a85.53 85.53 0 01-7.73-5.06c-2.7-1.8 8.83 2.72 6.13.7-8-4.35-22.2-13.84-25.88-17.22-.6-.56 6.22 2.18 5.6 1.62-38.5-31.7-33.7-53.13-32.5-66.57 1-10.75 8-24.52 19.75-30.1 5.7 3.1 9.24 5.48 9.24 5.48l-3.74-7.58c.46-.2.9-.15 1.36-.34 4.66 2.25 15 8.1 20.4 11.67 7.07 5 9.33 9.44 9.33 9.44s1.86-1 .48-5.37c-.5-1.78-2.65-7.45-9.65-13.17h.44a81.2 81.2 0 0111.87 8.24c2-7.18 5.53-14.68 4.75-28.1-.48-9.43-.26-11.87-1.92-15.5-1.5-3.13.83-4.35 3.42-1.1a32.83 32.83 0 00-2.21-7.4v-.24c3.23-11.24 68.25-40.46 73-43.88a67.36 67.36 0 0019.13-20.8c3.62-5.76 6.34-13.85 7-26.1.36-8.84-3.76-14.73-69.5-21.62-18-1.77-28.53-14.8-34.53-26.82-1.1-2.6-2.2-4.94-3.33-7.28a58 58 0 01-2.56-8.43c10.75-30.87 28.8-57 55.37-76.7 1.45-1.32-5.78.34-4.34-1 1.7-1.54 12.7-6 14.8-7 2.54-1.2-10.88-6.9-22.73-5.5-12.07 1.36-14.63 2.8-21.07 5.53 2.67-2.66 11.17-6.15 9.18-6.13-13 2-29.18 9.56-43 18.12a10.73 10.73 0 01.83-4.35c-6.44 2.73-22.26 13.8-26.87 23.14a44.33 44.33 0 00.27-5.4 84.48 84.48 0 00-13.19 13.82l-.24.22c-37.36-15-70.23-16-98.05-9.28-6.1-6.1-9.06-1.64-22.9-32.07-.94-1.83.72 1.8 0 0-2.28-5.9 1.4 7.87 0 0-23.28 18.37-53.92 39.2-68.63 53.9-.18.6 17.16-4.9 0 0-6 1.72-5.6 5.28-6.5 37.5-.22 2.44 0 5.18-.22 7.38-11.75 15-19.75 27.64-22.78 34.2-15.2 26.18-31.93 67-48.15 131.55a334.36 334.36 0 0125.79-50.32c-13.5 34.27-26.53 88.08-29.13 170.94a483.61 483.61 0 0112.53-50.66 473 473 0 0034.73 201.07c9.33 22.82 24.76 57.46 51 95.4C226.9 902 343.3 956 472.2 956c134.58 0 255.43-58.87 338.46-152.36z" />
      <path fill="url(#ff-D)" d="M711.1 866.7c162.87-18.86 235-186.7 142.38-190C769.85 674 634 875.6 711.1 866.7z" />
      <path fill="url(#ff-E)" d="M865.2 642.42C977.26 577.2 948 436.34 948 436.34s-43.25 50.24-72.62 130.32C846.4 646 797.84 681.8 865.2 642.42z" />
      <path fill="url(#ff-F)" d="M509.47 950.06C665.7 999.9 800 876.84 717.2 835.74 642 798.68 435.3 926.5 509.46 950.06z" />
      <path fill="url(#ff-G)" d="M876.85 702.23c3.8-5.36 8.94-22.53 13.48-30.2 27.58-44.52 27.78-80 27.78-80.84 16.66-83.22 15.15-117.2 4.9-180-8.25-50.6-44.32-123.1-75.57-158-32.2-36-9.5-24.25-40.7-50.52-27.33-30.3-53.82-60.3-68.25-72.36C634.22 43.1 636.57 24.58 638.6 21.4l-1.47 1.64C635.87 18.14 635 14 635 14s-57 57-69 152c-7.83 62 15.38 126.68 49 168a381.37 381.37 0 0059 58c25.4 36.48 39.38 81.5 39.38 129.9 0 121.24-98.34 219.53-219.65 219.53a220.45 220.45 0 01-49.13-5.52c-57.24-10.92-90.3-39.8-106.78-59.4-9.45-11.23-13.46-19.42-13.46-19.42 51.28 18.37 108 14.53 142.47-4.52 34.75-19.26 55.77-33.55 72.84-27.92 16.82 5.6 30.2-10.67 18.2-27.54-11.77-16.85-42.4-41-87.88-34.3-34.8 5.07-66.66 29.76-112.24 5.84a96.31 96.31 0 01-8.55-5c-3-1.77 9.77 2.7 6.8.68-8.87-4.32-24.57-13.73-28.64-17.07-.68-.56 6.88 2.16 6.2 1.6-42.62-31.45-37.3-52.7-36-66 1.07-10.66 8.8-24.32 21.86-29.86a152.3 152.3 0 0110.23 5.43l-4.14-7.5c.5-.2 1-.15 1.5-.34a268 268 0 0122.6 11.57c7.83 4.95 10.32 9.36 10.32 9.36s2.06-1 .54-5.33c-.56-1.77-2.93-7.4-10.68-13.07h.48a90.85 90.85 0 0113.13 8.17c2.2-7.12 6.12-14.56 5.25-27.86-.53-9.35-.28-11.78-2.12-15.4-1.65-3.1.92-4.3 3.78-1.1a30 30 0 00-2.44-7.34v-.24c3.57-11.14 75.53-40.12 80.77-43.5a70.31 70.31 0 0021.17-20.63c4-5.72 7-13.73 7.75-25.9.25-5.48-1.44-9.82-20.5-14-11.44-2.5-29.14-4.9-56.43-7.47-19.9-1.76-31.58-14.68-38.2-26.6-1.2-2.57-2.45-4.9-3.68-7.22a53.11 53.11 0 01-2.83-8.36 158.47 158.47 0 0161.28-76.06c1.6-1.3-6.4.33-4.8-1 1.87-1.52 14.06-5.93 16.37-6.92 2.8-1.2-12-6.84-25.16-5.47-13.36 1.35-16.2 2.78-23.32 5.5 3-2.64 12.37-6.1 10.16-6.08-14.4 2-32.3 9.48-47.6 18a9.68 9.68 0 01.92-4.31c-7.13 2.7-24.64 13.67-29.73 23a39.49 39.49 0 00.29-5.35 88.68 88.68 0 00-14.6 13.7l-.27.22c-41.3-14.9-77.68-15.9-108.43-9.2-6.74-6.06-17.57-15.23-32.9-45.4-1-1.82-1.6 3.75-2.4 2-6-13.8-9.55-36.44-9-52 0 0-12.32 5.6-22.5 29.06a152.9 152.9 0 01-4.32 8.87c-.56.68 1.27-7.7 1-7.24-1.77 3-6.36 7.2-8.37 12.62-1.38 4-3.32 6.27-4.56 11.3l-.3.46c-.1-1.48.37-6.08 0-5.14A236.1 236.1 0 0095.34 186c-5.5 18-11.88 42.6-12.9 74.57-.24 2.42 0 5.14-.25 7.32-13 14.83-21.86 27.4-25.2 33.9-16.8 26-35.33 66.44-53.3 130.46a319.14 319.14 0 0128.54-50C17.32 416.25 2.9 469.62 0 551.8a438.52 438.52 0 0113.87-50.24C11.3 556.36 17.68 624.3 52.32 701c20.57 45 67.92 136.6 183.62 208.05 0 0 39.36 29.3 107 51.26l15.23 5.33a93.43 93.43 0 01-4.7-2.05A484.88 484.88 0 00492.27 984c175.18.15 226.85-70.2 226.85-70.2l-.5.38q3.7-3.5 7.14-7.26C698.1 933 635 934.76 611.45 932.87c40.22-11.8 66.7-21.8 118.17-41.52q9-3.36 18.48-7.64l5.8-2.68a349.21 349.21 0 0070.26-44c51.7-41.3 62.95-81.56 68.83-108.1-.82 2.54-3.37 8.47-5.17 12.32-13.3 28.48-42.84 46-74.9 60.95a689 689 0 0042.38-62.44c10.48-10.37 13.7-26.6 21.56-37.53z" />
      <path fill="url(#ff-H)" d="M813.92 801c21.08-23.24 40-49.82 54.35-80 36.9-77.58 94-206.58 49-341.3C881.77 273.22 833 215 771.1 158.12 670.56 65.76 642.48 24.52 642.48 0c0 0-116.1 129.4-65.74 264.38s153.46 130 221.68 270.87c80.27 165.74-64.95 346.6-185 397.24 7.35-1.63 267-60.38 280.6-208.88-.35 2.73-6.2 43.8-80.1 77.4z" />
      <path fill="url(#ff-I)" d="M477.6 319.37c.4-8.77-4.16-14.66-76.68-21.46-29.84-2.76-41.26-30.33-44.75-41.94-10.6 27.56-15 56.5-12.64 91.48 1.6 22.92 17 47.52 24.37 62 0 0 1.64-2.13 2.4-2.9 13.86-14.43 71.94-36.42 77.4-39.54 6.02-3.84 28.9-20.56 29.92-47.63z" />
      <path fill="url(#ff-J)" d="M158.3 156.47c-1-1.82-1.6 3.75-2.4 2-6-13.8-9.58-36.2-8.72-52 0 0-12.32 5.6-22.5 29.06-1.9 4.2-3.1 6.54-4.32 8.86-.56.68 1.27-7.7 1-7.24-1.77 3-6.36 7.2-8.35 12.38-1.65 4.24-3.35 6.52-4.6 11.77-.4 1.43.4-6.32.05-5.38C84.72 201.68 80.2 271 82.7 268c50.48-53.86 108.3-66.64 108.3-66.64-6.15-4.53-19.53-17.63-32.7-44.9z" />
      <path fill="url(#ff-K)" d="M349.84 720.1c-69.72-29.77-149-71.75-146-167.14C207.92 427.35 321 452.18 321 452.18c-4.27 1-15.68 9.16-19.72 17.82-4.27 10.83-12.07 35.28 11.55 60.9 37.1 40.2-76.2 95.36 98.66 199.57 4.4 2.4-41-1.43-61.64-10.36z" />
      <path fill="url(#ff-L)" d="M325.07 657.5c49.44 17.2 107 14.2 141.52-4.86 23.1-12.85 52.7-33.43 70.92-28.35-15.78-6.24-27.73-9.15-42.1-9.86-2.45 0-5.38-.05-8-.32a136.23 136.23 0 00-15.76.86c-8.9.82-18.77 6.43-27.74 5.53-.48 0 8.7-3.77 8-3.6-4.75 1-9.92 1.2-15.37 1.88-3.47.4-6.45.82-9.9 1-103 8.73-190-55.8-190-55.8-7.4 25 33.17 74.3 88.52 93.57z" />
      <path fill="url(#ff-M)" d="M813.74 801.65c104.16-102.27 156.86-226.58 134.58-366 0 0 8.9 71.5-24.85 144.63 16.2-71.4 18.1-160.1-25-252C841 205.64 746.45 141.1 710.35 114.2c-54.7-40.8-77.35-82.32-77.78-90.9-16.34 33.48-65.77 148.2-5.3 247 56.64 92.56 145.86 120 208.33 204.95 115.08 156.42-21.85 326.4-21.85 326.4z" />
      <path fill="url(#ff-N)" d="M798.8 535.55C762.4 460.35 717 427.55 674 392c5 7 6.23 9.47 9 14 37.83 40.32 93.6 138.66 53.1 262.1C659.88 900.48 355 791.06 323 760.32c12.93 134.5 238 198.84 384.6 111.63C791 793 858.47 658.8 798.8 535.55z" />
    </svg>
  );
}

function Safari({ className }: IconProps) {
  return (
    <svg viewBox="0 0 5120 5120" className={className} role="img" aria-label="Safari">
      <defs>
        <linearGradient id="safari-a" x2="0" y2="100%">
          <stop offset="0" stopColor="#19d7ff" />
          <stop offset="1" stopColor="#1e64f0" />
        </linearGradient>
      </defs>
      <g fill="#ffffff">
        <circle cx="2560" cy="2560" r="2240" fill="url(#safari-a)" />
        <path fill="red" d="M4090 1020 2370 2370l4e2 4e2z" />
        <path d="M1020 4090l1350-1720 4e2 4e2z" />
      </g>
      {/* Compass rose tick marks */}
      <path fill="none" stroke="#ffffff" strokeWidth="30" d="M2560 540v330m0 3370v330m350-4e3-57 325m-586 3318-57 327M3250 662l-113 310M1984 4138l-113 310m339-3878 57 325m586 3318 57 327M1870 662l113 310m1152 3166 113 310M1552 810l166 286m1685 2918 165 286M1265 1010l212 253m2166 2582 212 253M1015 1258l253 212m2582 2168 253 212M813 1548l286 165m2920 1685 286 165M665 1866l310 113m3166 1150 310 113M574 2202l326 58m3320 588 325 57M545 2555h330m3370 0h330M575 2905l325-57m3320-586 325-57M668 3245l310-113m3165-1152 310-113M815 3563l286-165m2920-1685 286-165M1016 3850l253-212m2580-2166 253-212M1262 41e2l212-253m2166-2582 212-253M1552 43e2l166-286m1685-2918 165-286M2384 548l16 180m320 3656 16 180M2038 610l47 174m950 3544 47 174M1708 730l76 163m1550 3326 77 163M1404 904l103 148m2106 3006 103 148M1135 1130l127 127m2596 2596 127 127M910 14e2l148 103m3006 2107 146 1e2M734 1703l163 76m3326 1550 163 77M614 2033l174 47m3544 950 174 47M553 2380l180 16m3656 320 180 16m-4014 0 180-16m3656-320 180-16M614 3077l174-47m3544-950 174-47M734 3407l163-76m3326-1550 163-76M910 3710l148-103m3006-2107 146-1e2M1404 4206l103-148m2105-3006 104-148M1708 4380l77-163M3335 890l77-163M2038 45e2l47-174m950-3544 47-174m-698 3952 16-180m320-3656 16-180" />
    </svg>
  );
}

function Edge({ className }: IconProps) {
  return (
    <svg viewBox="0 0 512 512" className={className} role="img" aria-label="Edge">
      <defs>
        <radialGradient id="edge-a" cx=".6" cy=".5">
          <stop offset=".8" stopColor="#148" /><stop offset="1" stopColor="#137" />
        </radialGradient>
        <radialGradient id="edge-b" cx=".5" cy=".6" fx=".2" fy=".6">
          <stop offset=".8" stopColor="#38c" /><stop offset="1" stopColor="#269" />
        </radialGradient>
        <linearGradient id="edge-c" y1=".5" y2="1">
          <stop offset=".1" stopColor="#5ad" /><stop offset=".6" stopColor="#5c8" /><stop offset=".8" stopColor="#7d5" />
        </linearGradient>
      </defs>
      <path fill="url(#edge-a)" d="M439 374c-50 77-131 98-163 96-191-9-162-262-47-261-82 52 30 224 195 157 17-12 20 3 15 8" />
      <path fill="url(#edge-b)" d="M311 255c18-82-31-135-129-135S38 212 38 259c0 124 125 253 287 203-134 39-214-116-146-210 46-66 123-68 132 3 M411 99h1" />
      <path fill="url(#edge-c)" d="M39 253C51-15 419-30 472 202c14 107-86 149-166 115-42-26 26-20-3-99-48-112-251-103-264 35" />
    </svg>
  );
}

function Opera({ className }: IconProps) {
  return (
    <svg viewBox="0 0 1090 1090" className={className} role="img" aria-label="Opera">
      <defs>
        <linearGradient id="opera-a" x1="461" x2="461" y1="59" y2="1033" gradientUnits="userSpaceOnUse">
          <stop offset=".6" stopColor="#ff1b2d" /><stop offset="1" stopColor="#a70014" />
        </linearGradient>
        <linearGradient id="opera-b" x1="714" x2="714" y1="116" y2="978" gradientUnits="userSpaceOnUse">
          <stop offset="0" stopColor="#9c0000" /><stop offset=".7" stopColor="#ff4b4b" />
        </linearGradient>
      </defs>
      <path fill="url(#opera-a)" d="M545 42.5a502.5 502.5 0 10334.9 877.1 362.4 362.4 0 01-201.4 61.5c-119.7 0-226.8-59.4-299-153-55.6-65.6-91.5-162.5-94-271.3V533c2.5-108.8 38.4-205.8 94-271.3 72-93.6 179.3-153 299-153 73.6 0 142.5 22.5 201.4 61.6a500.8 500.8 0 00-333-127.9h-2z" />
      <path fill="url(#opera-b)" d="M379.6 261.8c46-54.4 105.7-87.3 170.7-87.3 146.3 0 265 166 265 370.4 0 204.6-118.6 370.4-265 370.4-65 0-124.6-32.8-170.7-87.2 72 93.6 179.2 153 299 153A363 363 0 00880 919.6 501 501 0 001047.5 545a501.1 501.1 0 00-167.6-374.6 362.4 362.4 0 00-201.4-61.5c-119.7 0-226.8 59.4-299 153" />
    </svg>
  );
}

function Brave({ className }: IconProps) {
  return (
    <svg viewBox="0 0 2770 2770" className={className} role="img" aria-label="Brave">
      <defs>
        <linearGradient id="brave-a" y1="51%" y2="51%">
          <stop offset=".4" stopColor="#f50" /><stop offset=".6" stopColor="#ff2000" />
        </linearGradient>
        <linearGradient id="brave-b" x1="2%" y1="51%" y2="51%">
          <stop offset="0" stopColor="#ff452a" /><stop offset="1" stopColor="#ff2000" />
        </linearGradient>
      </defs>
      <path fill="url(#brave-a)" d="M2395 723l60-147-170-176c-92-92-288-38-288-38l-222-252H992L769 363s-196-53-288 37L311 575l60 147-75 218 250 953c52 204 87 283 234 387l457 310c44 27 98 74 147 74s103-47 147-74l457-310c147-104 182-183 234-387l250-953z" />
      <path fill="#ffffff" d="M1935 524s287 347 287 420c0 75-36 94-72 133l-215 230c-20 20-63 54-38 113 25 60 60 134 20 210-40 77-110 128-155 120a820 820 0 01-190-90c-38-25-160-126-160-165s126-110 150-124c23-16 130-78 132-102s2-30-30-90-88-140-80-192c10-52 100-80 167-105l207-78c16-8 12-15-36-20-48-4-183-22-244-5s-163 43-173 57c-8 14-16 14-7 62l58 315c4 40 12 67-30 77-44 10-117 27-142 27s-99-17-142-27-35-37-30-77c4-40 48-268 57-315 10-48 1-48-7-62-10-14-113-40-174-57-60-17-196 1-244 6-48 4-52 10-36 20l207 77c66 25 158 53 167 105 10 53-47 132-80 192s-32 66-30 90 110 86 132 102c24 15 150 85 150 124s-119 140-159 165a820 820 0 01-190 90c-45 8-115-43-156-120-40-76-4-150 20-210 25-60-17-92-38-113l-215-230c-35-37-71-57-71-131s287-420 287-420l273 44c32 0 103-27 168-50 65-20 110-22 110-22s44 0 110 22 136 50 168 50c33 0 275-47 275-47zm-215 1328c18 10 7 32-10 44l-254 198c-20 20-52 50-73 50s-52-30-73-50a13200 13200 0 00-255-198c-16-12-27-33-10-44l150-80a870 870 0 01188-73c15 0 110 34 187 73l150 80z" />
      <path fill="url(#brave-b)" d="M1999 363l-224-253H992L769 363s-196-53-288 37c0 0 260-23 350 123l276 47c32 0 103-27 168-50 65-20 110-22 110-22s44 0 110 22 136 50 168 50c33 0 275-47 275-47 90-146 350-123 350-123-92-92-288-38-288-38" />
    </svg>
  );
}

function Samsung({ className }: IconProps) {
  return (
    <svg viewBox="0 0 24 24" className={className} role="img" aria-label="Samsung Internet">
      <defs>
        <linearGradient id="si-a" x1="0" y1="0" x2="1" y2="1">
          <stop offset="0%" stopColor="#8E4EEB" />
          <stop offset="100%" stopColor="#3B5BFE" />
        </linearGradient>
      </defs>
      <circle cx="12" cy="12" r="11" fill="url(#si-a)" />
      <path
        fill="#FFF"
        d="M16.8 8.6 C15.6 6.9 12.9 6.6 11 7.9 C9.4 9 9.2 11 10.6 12.1 C11.9 13.2 14.2 13.1 13.9 14.5
           C13.7 15.6 12 16 10.8 15.3 C10 14.9 9.6 14.2 9.5 13.4 L7.1 13.4 C7.3 15.4 8.7 16.9 10.8 17.3
           C13.3 17.8 15.9 16.6 16.4 14.4 C16.9 12.1 15 11 13.2 10.4 C12.2 10.1 11.4 9.8 11.6 9.2
           C11.9 8.4 13.4 8.3 14.3 9 C14.6 9.2 14.8 9.5 14.9 9.8 Z"
      />
    </svg>
  );
}

function InternetExplorer({ className }: IconProps) {
  return (
    <svg viewBox="0 0 24 24" className={className} role="img" aria-label="Internet Explorer">
      <circle cx="12" cy="12" r="11" fill="#1EBBEE" />
      <path
        fill="#FFF"
        d="M12 5.4 C8.4 5.4 5.5 8.2 5.3 11.7 L18.7 11.7 C18.5 8.2 15.6 5.4 12 5.4 Z
           M5.5 13.4 C6.2 16.3 8.8 18.5 12 18.5 C14.4 18.5 16.5 17.3 17.7 15.4 L15.1 15.4
           C14.4 16.3 13.3 16.9 12 16.9 C10.1 16.9 8.5 15.4 8.2 13.4 Z"
      />
    </svg>
  );
}

/** Generic globe — unrecognised or "Other" user agents. */
function Globe({ className }: IconProps) {
  return (
    <svg viewBox="0 0 24 24" className={className} role="img" aria-label="Other browser">
      <circle cx="12" cy="12" r="10.5" fill="none" stroke="currentColor" strokeWidth="1.6" opacity="0.55" />
      <ellipse cx="12" cy="12" rx="4.4" ry="10.5" fill="none" stroke="currentColor" strokeWidth="1.6" opacity="0.55" />
      <path d="M1.9 8.6h20.2M1.9 15.4h20.2" stroke="currentColor" strokeWidth="1.6" opacity="0.55" />
    </svg>
  );
}

/** Terminal — scripted clients (curl, Python, Go). */
function Terminal({ className, accent }: IconProps & { accent: string }) {
  return (
    <svg viewBox="0 0 24 24" className={className} role="img" aria-label="Scripted client">
      <rect x="1.5" y="3.5" width="21" height="17" rx="3" fill={accent} />
      <path
        d="M6.5 9.5 L9.5 12 L6.5 14.5 M12 15.5 H17"
        fill="none"
        stroke="#FFF"
        strokeWidth="1.8"
        strokeLinecap="round"
        strokeLinejoin="round"
      />
    </svg>
  );
}

const ICONS: Record<string, (p: IconProps) => React.ReactElement> = {
  Chrome,
  Firefox,
  Safari,
  Edge,
  Opera,
  Brave,
  Samsung,
  IE: InternetExplorer,
  curl: (p) => <Terminal {...p} accent="#073551" />,
  Python: (p) => <Terminal {...p} accent="#3776AB" />,
  Go: (p) => <Terminal {...p} accent="#00ADD8" />,
};

export function BrowserIcon({ name, className = "h-4 w-4" }: { name: string; className?: string }) {
  const Icon = ICONS[name] ?? Globe;
  return <Icon className={`${className} shrink-0`} />;
}

/** Bar colour for a browser, so the breakdown bars carry the brand too. */
export const BROWSER_COLORS: Record<string, string> = {
  Chrome: "#4285F4",
  Firefox: "#FF7139",
  Safari: "#1E9BF0",
  Edge: "#3B8CC4",
  Opera: "#FF1B2D",
  Brave: "#FF5500",
  Samsung: "#6C4BE0",
  IE: "#1EBBEE",
  curl: "#073551",
  Python: "#3776AB",
  Go: "#00ADD8",
};
