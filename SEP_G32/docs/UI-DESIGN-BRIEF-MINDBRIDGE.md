# UI Design Brief: MindBridge

Use this brief when designing the OPCBS Razor Pages frontend.

## Role

Act as a senior UI/UX designer with 10+ years of experience in healthcare and mental health platforms.

The task is to transform existing low-fidelity/skeleton Razor Pages and `docs/ui-template` references into high-fidelity, production-ready UI.

The wireframes/templates/specifications define business requirements and layout intent. Do not change business functionality, user flow, fields, actions, menus, buttons, forms, or navigation structure unless another OPCBS spec explicitly requires it.

You may improve visual design, hierarchy, responsiveness, spacing, components, and interaction polish.

## Brand Identity

Product name: MindBridge

Tagline: Connecting Minds, Supporting Wellbeing

MindBridge is a professional mental health counseling platform that connects individuals with qualified mental health professionals.

The brand should communicate:

- Trust and safety
- Emotional support
- Professional expertise
- Human connection
- Personal growth
- Mental wellness
- Accessibility

The experience should make users feel:

- Welcome
- Calm
- Understood
- Supported
- Safe to seek help

## Visual Inspiration

Use BetterHelp as the primary visual inspiration and Psychology Today as inspiration for article/blog cards and therapist discovery patterns.

Do not copy either brand exactly. Create an original MindBridge identity suitable for a healthcare technology platform.

Visual cues may suggest:

- Connection
- Guidance
- Growth
- Balance
- Wellness
- Hope
- A bridge between people seeking support and professionals providing care

Avoid styles that feel like:

- Clinical hospital systems
- Corporate enterprise software
- Banking or financial services
- Social media applications
- Gaming interfaces
- Dark dashboards
- Heavy gradients
- Excessive animations

## Homepage Messaging

Primary headline:

```text
Connect with the Right Mental Health Professional
```

Supporting copy:

```text
MindBridge helps individuals access trusted mental health support through professional counseling, personalized care, and meaningful human connection.
```

Tone of voice:

- Warm
- Reassuring
- Professional
- Empathetic
- Clear
- Trustworthy

## Color Palette

Use these tokens consistently in CSS:

```text
Primary: #4E8B70
Primary Hover: #3F735C
Secondary: #DCEEE4
Background: #FAFBFA
Card Background: #FFFFFF
Border: #E5E7EB
Primary Text: #1F2937
Secondary Text: #6B7280
Success: #10B981
Warning: #F59E0B
Error: #EF4444
```

## Typography

Use Inter as the primary font.

- Headings: 600-700 weight
- Body: 400-500 weight
- Clear hierarchy
- Strong readability
- Accessible color contrast

## Components

Buttons:

- 12px border radius
- Medium height
- Soft shadow
- Clear hover/focus states

Cards:

- 20px border radius for content cards
- White background
- Soft shadow
- Calm spacing

Input fields:

- 12px border radius
- Clean border
- Clear validation states

Tags/chips:

- Pill style
- Fully rounded

Navigation:

- Clean top navigation
- Minimal icons
- Role-aware menus

Profile cards:

- Professional healthcare appearance
- Clear doctor name, title, specialties, rating, experience, and call-to-action

Blog cards:

- Psychology Today style article cards
- Clear image, title, category, author, excerpt, and date

Dashboards:

- Light, calm, professional
- Avoid dark dashboard styling
- Dense enough for operations, but not visually heavy

## Layout Principles

- Calm and trustworthy
- Modern healthcare platform
- Clean and minimal
- Human-centered
- Large whitespace
- Soft rounded corners
- Friendly but professional
- Responsive desktop-first design with good mobile behavior

## Constraints

Keep:

- All pages required by OPCBS docs
- All menus required by role/permission docs
- All fields
- All buttons
- All forms
- All user actions
- All workflows
- All business logic

Do not:

- Add unrelated features
- Remove required features
- Rearrange workflows in ways that change business meaning
- Invent additional fields not backed by API/spec
- Put business rules in Razor views

Only improve visual design and frontend usability while preserving the OPCBS requirements.
