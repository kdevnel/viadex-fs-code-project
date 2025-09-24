# Viadex Device Portal - Production Setup Guide

## Architecture Overview

This is a **full-stack device leasing and customer portal application** built with modern web technologies and clean architecture principles.

## Setup Instructions
You can read detailed setup instructions at [SETUP.md](./SETUP.md)

## General Notes

- This repo demonstrates strong version control principles. Code was built on feature branches off develop at first and merged through Pull Requests to demonstrate fluency. This is generally overkill on a project of this size with one engineer.
- I heavily relied on AI as a code assistant to bridge any gap between my fundamental knowledge and my lack of experience in the specific tech stack. I see this is a strength as I can accomplish far more when I'm augmented.
- I engineered the app with more complexity than I normally would in order to properly understand how I would architect this in .NET/Vue. Following a successful offer, I would work on a strong early days plan to upskill myself in the tech stack during my probation. That said, I do believe it's important to show strong planning for growth when building an app foundation. Added complexities in the backend are useful down the line.
- Purchase date is automatically defined in the DeviceService to show auto-creation of data at creation of a database entity. Other fields require some input.
- I have shown two levels of validation for monthly price based on a maximum value of £10,000. I have added a DB constraint to ensure a maximum of 99,999.00 in that field. I have also validated the value in the Devices DTO to fit the business requirement. You can see this in shipments too.
- Validations applied to database fields and automatic value setting on fields such as delivery day.
- Quote calculated as `((Monthly * Tier Modifier) + Monthly Rate) * duration`
- I've implemented display of only active devices on the quote form
- There is a small gap in the way the app pulls the active devices. The API is paged so I manually increased the page size artificially for now. In production I would ensure there was a way to return all active devices for that list. As that grew, we would need to handle potentially large active device lists in a good way. Perhaps a searchable dropdown with caching for example.
- Charts added using Chart.js for simplicity. Charts added as components to showcase good project organisation.

## Architecture Notes

### Backend Design Patterns
- **Service Layer Architecture**: Controllers → Services → Data Access for clean separation of concerns
- **Result Pattern**: Structured error handling without exceptions for business logic failures
- **DTO/Entity Mapping**: Clean API contracts separate from domain models via extension methods
- **Multi-Layer Validation**: Data annotations + business rules + database constraints

### Frontend Architecture
- **Unified Component Patterns**: Consistent structure across all views with status cards, filtering, and pagination
- **State Persistence**: Pinia stores with localStorage for persistence of user preferences and filter selections
- **Native Fetch Client**: Zero-dependency API client (avoiding Axios) tailored to backend Result patterns
- **Responsive CSS Grid**: Modern table layouts replacing traditional HTML tables

### Database Strategy
- **Migrations**: Version-controlled schema changes with EF Core
- **Seeding**: CLI commands for flexible data management during development

### Error Handling
- **Backend**: Result pattern with structured success/failure responses
- **Frontend**: Centralized error handling with user-friendly messaging
- **API**: Multi-layer validation with clear 400/404/500 responses

### Testing methodology
- **Backend**: None implemented due to a lack of prior experience with C#/.NET experience. However, I would implement tests to verify all necessary business logic and API structure/functionality.
- **Frontend**: Sample tests implemented in `./device-portal-web/src/__tests__` to show testing of frontend elements and API mocking.

## Production Deployment Notes

- Implement authentication/authorization
- Add rate limiting
- Configure proper CORS origins
- Enable security headers

### Performance Optimization
- Add caching for read-heavy endpoints
- Implement database indexing strategy